namespace Tefin.Core

open System.IO
open System.Threading.Tasks

type ITextWriter =
    abstract WriteAsync: text: string -> Task
    abstract WriteLineAsync: text: string -> Task
    abstract WriteLineAsync: unit -> Task
    abstract DisposeAsync: unit -> Task
//abstract Init : writer:StreamWriter -> unit

module Writer =
    let writerIO (writer: StreamWriter) =
        //let mutable writer:StreamWriter = Unchecked.defaultof<StreamWriter>
        { new ITextWriter with
            member x.WriteAsync text = task { do! writer.WriteAsync text }
            member x.WriteLineAsync text = task { do! writer.WriteLineAsync text }
            member x.WriteLineAsync() = task { do! writer.WriteLineAsync() }
            member x.DisposeAsync() = task { do! writer.DisposeAsync() } }
