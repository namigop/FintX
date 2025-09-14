namespace Tefin.Core.Scripting

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Security.Cryptography
open System.Text
open System.Threading.Tasks
open Microsoft.CodeAnalysis.CSharp.Scripting
open System.Runtime.Loader
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Scripting
open Microsoft.CodeAnalysis.Scripting
open Tefin.Core.Reflection
open Tefin.Core;

type ScriptGlobals(id:string) =
    member _.Context =  Dictionary<string, obj>()  
    member _.Id = id
    
type ScriptEngine =
    { Id : string
      Runners  : ConcurrentDictionary<string, ScriptRunner<obj>>
      Globals : ScriptGlobals }
      interface IDisposable with
         member x.Dispose() =
           x.Runners.Clear()
           x.Globals.Context.Clear()

module Script =
    type T =
        { Full : string
          Runnable : string}
        
    let private getScriptHash (code:string) =
        let md5 = MD5.Create()
        let bytes = md5.ComputeHash (Encoding.UTF8.GetBytes code)
        Convert.ToBase64String bytes
        
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
    let private compileCsScript (code:string) =        
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
                
        let script = CSharpScript.Create(code, scriptOptions, globalsType=typeof<ScriptGlobals>)       
        let runner = script.CreateDelegate()
        runner
    
    let private compile code =
        try
            let runner = compileCsScript code
            Res.ok runner
        with
        | exc -> Res.failed exc
     
    let createEngine (id) =
        { Id = id
          Runners = new ConcurrentDictionary<string, ScriptRunner<obj>>()
          Globals = ScriptGlobals(id)}
    
    let dispose (engine: ScriptEngine) =
       engine :> IDisposable
       |> _.Dispose()
       
    let private runInternal (io:IOs) (engine:ScriptEngine) code =
      task {
        let hash = getScriptHash code
        let runner = engine.Runners.GetOrAdd (hash, fun _ ->
            let res = compile code            
            if (res.IsError) then
                let exc = Res.getError res
                raise exc               
            Res.getValue res )

        let! scriptResult = runner.Invoke(engine.Globals)
        let! stringResult =
            task {
                if scriptResult = null then
                    return "<null>"
                else if (TypeHelper.isOfType (scriptResult.GetType()) (typeof<Task>)) then
                    let t = scriptResult :?> Task
                    do! t
                    return 
                        t.GetType().GetProperty "Result"
                        |> fun prop -> prop.GetValue t
                        |> fun x -> if (x = null) then "<null>" else x.ToString()
                else
                    return scriptResult.ToString()
             }
            
        io.Log.Info($"Executing {code} ==> {stringResult}")
        return stringResult                
     }
    
    let run (io:IOs) (engine:ScriptEngine) code =        
        Res.execTask (fun () -> runInternal io engine code)
        |> Res.mapTask (fun t -> Task.FromResult (Ret.Ok t))
    