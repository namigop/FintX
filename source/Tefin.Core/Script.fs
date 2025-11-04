namespace Tefin.Core.Scripting

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Security.Cryptography
open System.Text
open System.Threading.Tasks
open Microsoft.CodeAnalysis.CSharp.Scripting
open Microsoft.CodeAnalysis.Scripting
open Tefin.Core.Reflection
open Tefin.Core;

type SingleScript = {
    IsSelected : bool
    Content : string
}
type ScriptFile = {
    ServiceType : string
    Method : string
    Scripts : SingleScript array
}

type ScriptEngine =
    { Id : string
      Runners  : ConcurrentDictionary<string, ScriptRunner<obj>> }
      interface IDisposable with
         member x.Dispose() =
           x.Runners.Clear()
           
module Script =
    type T =
        { Full : string
          Runnable : string}
        
    let private getScriptHash (code:string) =
        let md5 = MD5.Create()
        let bytes = md5.ComputeHash (Encoding.UTF8.GetBytes code)
        Convert.ToBase64String bytes
        
    let generateDefaultScriptText (mi:MethodInfo) =
        let getReturnType (retType : Type)=
            if (retType.IsGenericType && retType.GetGenericTypeDefinition() = typedefof<Task<_>>) then
                retType.GetGenericArguments()[0]
            else
                retType
                
        let struct (created, resp) = TypeBuilder.getDefault (getReturnType mi.ReturnType) true None 0
        let json = Instance.indirectSerialize (getReturnType(mi.ReturnType)) resp
        use sr = new StringReader(json)
        
        let mutable l = sr.ReadLine()
        let mutable stringSampleLine = ""
        let mutable  intSampleLine = ""
        let mutable  doubleSampleLine = ""
        while (l <> null) do
            if (stringSampleLine = "" && l.Contains(": \"\"")) then
                stringSampleLine <- l
            
            if (intSampleLine = "" && l.Contains(": 0")) then
                intSampleLine <- l
            
            if (doubleSampleLine = "" && l.Contains(": 0.0")) then
                doubleSampleLine <- l
                
            l <- sr.ReadLine()
        

        let sb =  StringBuilder("// Add C# snippets to the json below to make more dynamic");
        sb.AppendLine() |> ignore
        if not (String.IsNullOrWhiteSpace(stringSampleLine)) then
            stringSampleLine <- stringSampleLine.Trim().Replace("\"\"", "\"$<<DateTime.Now.ToString(\"yyyy-MM-dd HH:mm:ss.fff\")>>\"")
            sb.AppendLine($"// For example: {stringSampleLine}") |> ignore
        
        if not (String.IsNullOrWhiteSpace(intSampleLine)) then
            intSampleLine <- intSampleLine.Trim().Replace("0", "$<<Random.Shared.Next(1,100)>>")
            sb.AppendLine($"// For example: {intSampleLine}") |> ignore
        
        if not (String.IsNullOrWhiteSpace(doubleSampleLine)) then
            let demoFunc =
                """$<<
//                   var x = 2.0;
//                   double WhatIsLife() {
//                     return 21 * x;
//                   }
//                   return WhatIsLife();
//                >>"""
            doubleSampleLine <- doubleSampleLine.Trim().Replace("0.0", $"{demoFunc}")
            sb.AppendLine($"// Multi-line snippets are fine too. For example: \n" +
                          $"// {doubleSampleLine}").AppendLine() |> ignore
        
        sb.AppendLine(json) |> ignore
        sb.ToString()
        
    /// Compiles a given C# script into a ScriptRunner delegate for execution.
    ///
    /// Parameters:
    ///   code: The C# code to be compiled as a script.
    ///
    /// Returns:
    ///   A delegate that can be invoked to execute the compiled script logic.
    ///
    /// Remarks:
    ///   The compilation uses Microsoft.CodeAnalysis.CSharp.Scripting to create a script execution environment.
    ///   The script is compiled with default options that include assemblies and namespaces commonly used
    ///   in .NET programming. For example, `System`, `System.Text`, `System.Linq` etc., are automatically
    ///   imported. It is designed to support scenarios where dynamic scripting based on C# code may be required.
    let private compileCsScript (code:string) globalsType =        
        let scriptOptions =
            ScriptOptions.Default.WithReferences(
                typeof<obj>.Assembly, // mscorlib
                typeof<System.IO.File>.Assembly,                     
                typeof<Console>.Assembly,
                typeof<System.Text.RegularExpressions.Regex>.Assembly,
                typeof<System.Text.Json.JsonDocument>.Assembly,
                typeof<System.Linq.Enumerable>.Assembly,    
                typedefof<List<_>>.Assembly
            ).WithImports(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "System.Text.Json",
                "System.IO",
                "System.Text.RegularExpressions"
            )
                
        let script = CSharpScript.Create(code, scriptOptions, globalsType)       
        let runner = script.CreateDelegate()
        runner
    
    let compile code globalsType =
        try
            let runner = compileCsScript code globalsType
            Res.ok runner
        with
        | exc -> Res.failed exc
     
    let createEngine (id) =
        { Id = id
          Runners = new ConcurrentDictionary<string, ScriptRunner<obj>>() }
    let dispose (engine: ScriptEngine) =
       engine :> IDisposable
       |> _.Dispose()
       
    let getOrAddRunner (engine:ScriptEngine) (code:string) globalsType =
        let hash = getScriptHash code
        let runner = engine.Runners.GetOrAdd (hash, fun _ ->
            let res = compile code globalsType        
            if (res.IsError) then
                let exc = Res.getError res
                raise exc               
            Res.getValue res )
        runner
    let private runInternal (io:IOs) (engine:ScriptEngine) (scriptGlobal:obj) code =
      task {        
        let runner = getOrAddRunner engine code (scriptGlobal.GetType())
        let! scriptResult = runner.Invoke(scriptGlobal)
        let nullMarker = "<null>"
        let! stringResult =
            task {                
                if scriptResult = null then
                    return nullMarker
                else if (TypeHelper.isOfType (scriptResult.GetType()) (typeof<Task>)) then
                    let t = scriptResult :?> Task
                    do! t
                    return 
                        t.GetType().GetProperty "Result"
                        |> fun prop -> prop.GetValue t
                        |> fun x -> if (x = null) then nullMarker else x.ToString()
                else
                    return scriptResult.ToString()
             }
            
        io.Log.Info($"Executing {code} ==> {stringResult}")        
        if (stringResult = nullMarker) then
            io.Log.Warn($"Script returned {nullMarker}! Will be ignored")
            return ""
        else
            return stringResult                               
     }
    
    let run (io:IOs) (engine:ScriptEngine) scriptGlobals code =        
        Res.execTask (fun () -> runInternal io engine scriptGlobals code)
        |> Res.mapTask (fun t -> Task.FromResult (Ret.Ok t))
    