namespace Tefin.Core.Scripting

open System
open System.Net.Security
open System.Text
open Tefin.Core
module ScriptExec =
    
    let start (io:IOs) (scriptText : string) id=
        io.Log.Debug($"Parsing script: {scriptText}")
        ScriptParser.parse scriptText
        |> Res.map (fun parsedLines ->
            task {
                let sb = StringBuilder()
                let engine = Script.createEngine id
                for line in parsedLines do
                    if line.ContainsScript then
                        let script = ScriptParser.extract line.Raw
                        let! result = Script.run io engine script.Runnable
                        if result.IsError then
                            raise (Res.getError result)
                            
                        let scriptResult = Res.getValue result                            
                        sb.AppendLine(line.Raw.Replace(script.Full, scriptResult) ) |> ignore
                    else if line.IsComment then
                        ()
                    else
                        sb.AppendLine(line.Raw) |> ignore
                
                let finalOutput = sb.ToString()
                io.Log.Info($"Script output:\n{finalOutput}")
                return finalOutput
            })
        |> Res.unwrapTask         

