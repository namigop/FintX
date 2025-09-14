namespace Tefin.Core.Scripting

open System
open System.IO
open Tefin.Core
open System.Text
open Script

module ScriptParser =
    type MultiLineParseState =
        { LineStart : int
          Builder : StringBuilder option            
        }
    
    let defaultExpressionStart = "$<<"
    let defaultExpressionEnd = ">>"
    
    let extractScriptWithExpr (exprStart : string) (exprEnd : string) (raw:string) = 
        let startPos = raw.IndexOf(exprStart, StringComparison.Ordinal)
        let endPos = raw.IndexOf(exprEnd, StringComparison.Ordinal)
        let script = raw.Substring(startPos + exprStart.Length, endPos - startPos - exprStart.Length)
        let full = raw.Substring(startPos, endPos - startPos + exprEnd.Length)
        { Full = full; Runnable = script }
    
    let extract = extractScriptWithExpr defaultExpressionStart defaultExpressionEnd
        
    let parseWithExpr (exprStart : string) (exprEnd : string) (text:string)  =
        let expressionStart =
            if (String.IsNullOrWhiteSpace exprStart) then
                defaultExpressionStart
            else
                exprStart
        let expressionEnd =
            if (String.IsNullOrWhiteSpace exprEnd) then
                defaultExpressionEnd
            else
                exprEnd
        
        Res.exec (fun () ->
            if (String.IsNullOrWhiteSpace text) then
                failwith "Empty script"
          
            text)
        |> Res.map (fun x ->
            use reader = new StringReader(x)
            let lines = ResizeArray<string>()
            let mutable line = reader.ReadLine()
            while line <> null do
                lines.Add(line)
                line <- reader.ReadLine()
            lines.ToArray())
        |> Res.map (fun lines ->
            lines
            |> Array.mapFold (fun state line ->
                let (lineNumber, sb)= state
                if line.TrimStart().StartsWith("//") then
                    let scr = ScriptLine.asComment line lineNumber                    
                    (scr, (lineNumber + 1, None))
                else if line.Contains(expressionStart) && line.Contains(expressionEnd) then
                    let scr = ScriptLine.asOneliner line lineNumber                   
                    (scr, (lineNumber + 1, None))
                else if line.Contains(expressionStart) && not (line.Contains(expressionEnd)) then
                    //start of a multiline expression
                    let scr = ScriptLine.asMultilineScript line lineNumber  -1
                    let builder = StringBuilder(line).AppendLine()
                    let s = Some (lineNumber, builder)
                    (scr, (lineNumber + 1, s))
                else if (sb.IsSome) && not(line.Contains(expressionEnd)) then
                    //body of the multiline expression
                    let (multiLineStart, builder:StringBuilder) = sb.Value
                    ignore(builder.AppendLine line)
                    let scr = ScriptLine.asMultilineScript line lineNumber  -1
                    let s = Some (multiLineStart, builder)
                    (scr, (lineNumber + 1, s))
                else if (sb.IsSome) && line.Contains(expressionEnd) then
                    //End of the multiline expression
                    let (multiLineStart, builder) = sb.Value
                    ignore(builder.AppendLine line)
                    let scr = ScriptLine.asMultilineScript (builder.ToString()) multiLineStart lineNumber
                    (scr, (lineNumber + 1, None))
                else
                    let scr = ScriptLine.asRaw line lineNumber
                    (scr, (lineNumber + 1, None))
                ) (0, None))
        |> Res.map (fun (scrLines, _) -> scrLines |> Array.filter (fun l -> l.LineEnd > -1))
    
    let parse = parseWithExpr defaultExpressionStart defaultExpressionEnd
            
           
