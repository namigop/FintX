namespace Tefin.Core

open System.Collections.Generic
open System.IO
open Tefin.Core.Interop

module AutoSave =

    type FileParam =
        { Json: string
          Header : string
          FullPath: string option }

        static member Empty() = { Json = ""; Header = ""; FullPath = None }
        member x.WithJson j = { x with Json = j }
        member x.WithHeader h  = { x with Header = h }
        member x.WithFullPath p = { x with FullPath = p }

    type MethodParam =
        { Name: string
          Files: FileParam array }

        static member Empty() = { Name = ""; Files = Array.empty }
        member x.WithName n = { x with Name = n }
        member x.WithFiles f = { x with Files = f }

    type ClientParam =
        { Project: Project
          Client: ClientGroup
          Methods: MethodParam array }

        static member Empty() =
            { Project = Unchecked.defaultof<Project>
              Client = Unchecked.defaultof<ClientGroup>
              Methods = Array.empty }

        member x.WithProject p = { x with Project = p }
        member x.WithClient c = { x with Client = c }
        member x.WithMethods m = { x with Methods = m }


    let rec private saveFile (methodName: string) (methodPath: string) (f: FileParam) (nameCache:Dictionary<string, string>) =
        let write (file:string) (json:string)=
         try
             File.WriteAllText(file, json)
         with exc -> ()
            
        match f.FullPath with
        | Some file -> write file f.Json
        
        | None ->
            let key = $"{methodPath}/{f.Header}"
            let fileName =
              
                let found, prevName = nameCache.TryGetValue key
                if found then
                    prevName
                else
                    let existingFileNames =
                        Directory.GetFiles(methodPath, "*" + Ext.requestFileExt)
                        |> Array.map (fun c -> Path.GetFileName c)

                    let max = 1000000

                    seq {
                        for i in 1..max do
                            i
                    }
                    |> Seq.map (fun counter ->
                        //Sample name : MethodName (1).frxq
                        let targetName = $"{methodName} ({counter}){Ext.requestFileExt}"
                        targetName)
                    |> Seq.filter (fun name ->
                        let existingFile = existingFileNames |> Array.contains name
                        not existingFile)
                    |> Seq.head
                    use the header name
            let fullPath = Path.Combine (methodPath, fileName)
            nameCache[key] <- fullPath
            write fullPath f.Json
            


    let private saveMethod (clientPath: string) (m: MethodParam) (nameCache:Dictionary<string, string>) =
        let methodPath = Path.Combine(clientPath, "methods", m.Name)
        let _ = Directory.CreateDirectory methodPath

        for f in m.Files do
            saveFile m.Name methodPath f nameCache

    let private saveClient (clientParam: ClientParam) (nameCache:Dictionary<string, string>) =
        if (Directory.Exists clientParam.Client.Path) then
            for m in clientParam.Methods do
                saveMethod clientParam.Client.Path m nameCache

    let run =
        let timer = new System.Timers.Timer()
        timer.AutoReset <- true
        timer.Interval <- 5000 //5 sec
        timer.Enabled <- true

        let nameCache = Dictionary<string, string>()
        fun (getParam: System.Func<ClientParam array>) ->
            timer.Elapsed
            |> Observable.add (fun args ->
                let clientParams = getParam.Invoke()

                for clientParam in clientParams do
                    saveClient clientParam nameCache

                )
