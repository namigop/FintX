namespace Tefin.Core

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Security.Cryptography
open System.Text
open Microsoft.CodeAnalysis.CSharp.Scripting
open System.Runtime.Loader
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Scripting
open Microsoft.CodeAnalysis.Scripting
open Tefin.Core.Utils;

type ScriptGlobals(id:string) =
    member _.Shared =  Dictionary<string, obj>()  
    member _.Id = id
    
type ScriptEngine =
    { Id : string
      Runners  : ConcurrentDictionary<string, ScriptRunner<obj>>
      Globals : ScriptGlobals }
      interface IDisposable with
         member x.Dispose() =
           x.Runners.Clear()
           x.Globals.Shared.Clear()


module Script =    
    let private getScriptHash (code:string) =
        let md5 = MD5.Create()
        let bytes = md5.ComputeHash (Encoding.UTF8.GetBytes code)
        Convert.ToBase64String bytes
        
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
                "System.Text.RegularExpressions"
            )
                
        let script = CSharpScript.Create(code, scriptOptions, globalsType=typeof<ScriptGlobals>)       
        let runner = script.CreateDelegate()
        runner
    
    let compile code =
        try
            let runner = compileCsScript code
            Res.ok runner
        with
        | exc -> Res.failed exc
     
    let createEngine (id) =
        { Id = id
          Runners = new ConcurrentDictionary<string, ScriptRunner<obj>>()
          Globals = ScriptGlobals(id)}
    
    let run (engine:ScriptEngine) code =
        let hash = getMD5Hash code
        let runner = engine.Runners.GetOrAdd (hash, fun _ ->
            let res = compile code            
            if (res.IsError) then
                let exc = Res.getError res
                raise exc
                
            Res.getValue res )
        continue.
        ()
        