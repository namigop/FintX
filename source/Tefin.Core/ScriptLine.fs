namespace Tefin.Core.Scripting

module ScriptLine =
   type T =
    { LineStart : int
      LineEnd : int
      Raw : string
      IsComment : bool
      ContainsScript : bool }
    
   let asComment (raw: string) (lineNumber:int) =
        { LineStart = lineNumber
          LineEnd = lineNumber
          Raw = raw
          IsComment = true
          ContainsScript = false }
        
   let asRaw (raw: string) (lineNumber:int) =
        { LineStart = lineNumber
          LineEnd = lineNumber
          Raw = raw
          IsComment = false
          ContainsScript = false }
        
   let asOneliner (raw: string) (lineNumber:int) =
        { LineStart = lineNumber
          LineEnd = lineNumber
          Raw = raw
          IsComment = false
          ContainsScript = true }
        
   let asMultilineScript (raw: string) (lineStart:int) (lineEnd:int)=
        { LineStart = lineStart
          LineEnd = lineEnd
          Raw = raw
          IsComment = false
          ContainsScript = true }

    

