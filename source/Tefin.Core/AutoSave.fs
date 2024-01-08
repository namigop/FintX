namespace Tefin.Core

open System.Collections.Generic
open System.IO
open System.Reflection
open Tefin.Core.Interop

module AutoSave =

    type FileParam =
        { Json: string
          Header : string
          FullPath: string option }

        static member Empty() = { Json = ""; Header = ""; FullPath = None }
        member x.WithJson j = { x with Json = j }
        member x.WithHeader h  = { x with Header = h }
        member x.WithFullPath p = { x with FullPath = if (System.String.IsNullOrWhiteSpace p) then None else Some p }

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


   
    let rec private saveFile (io:IOResolver) (methodName: string) (methodPath: string) (f: FileParam) =
        let write (file:string) (json:string)=
         try
             io.File.WriteAllText file json
         with exc ->
             io.Log.Warn ($"Unable to auto-save {Path.GetFileName(file)}. {exc.Message}");
            
        match f.FullPath with
        | Some file ->
            write file f.Json
            f
        | None ->
            // if the json content was provided but no full path, we save it with the
            // next available file name
            let fileName = Utils.getAvailableFileName methodPath f.Header Ext.requestFileExt         
            let fullPath = Path.Combine (methodPath, fileName)            
            write fullPath f.Json
            { Json = f.Json
              Header = f.Header
              FullPath = Some fullPath }

    let private saveMethod (io:IOResolver) (clientPath: string) (m: MethodParam)  =
        
        let methodPath =
            Project.getMethodPath clientPath
            |> fun p -> Path.Combine(p, m.Name, Project.autoSaveFolderName)
            
        io.Dir.CreateDirectory methodPath
   
        let autoSavedFiles = 
            m.Files
            |> Array.map (fun fileParam -> saveFile io m.Name methodPath fileParam)
            |> Array.map (fun p -> p.FullPath.Value)
        
        //Delete any existing files that were not auto-saved
        let existingFiles = io.Dir.GetFiles methodPath
        for e in existingFiles do
            if not (Array.contains e autoSavedFiles) then
                io.File.Delete e

    let private saveClient (io:IOResolver) (clientParam: ClientParam)   =
        if (Directory.Exists clientParam.Client.Path) then
            for m in clientParam.Methods do
                saveMethod io clientParam.Client.Path m 

    let getSaveLocation (io:IOResolver) (methodInfo:MethodInfo) (clientPath:string) =
        let methodName = methodInfo.Name
        let autoSavePath = Project.getMethodPath(clientPath) |> fun p -> Path.Combine(p, methodName, Project.autoSaveFolderName)
        io.Dir.CreateDirectory autoSavePath
        let fileName = Utils.getAvailableFileName autoSavePath methodName Ext.requestFileExt
        let fullPath = Path.Combine(autoSavePath, fileName)
        if not (io.File.Exists fullPath) then 
            io.File.WriteAllText fullPath ""
        fullPath
    
    let getAutoSavedFiles (io:IOResolver) (clientPath:string) =
        Project.getMethodPath clientPath
        |> fun path -> io.Dir.GetFiles(path, "*"+Ext.requestFileExt, SearchOption.AllDirectories)
        |> Array.filter (fun fp -> fp.Contains(Project.autoSaveFolderName))
        |> Array.sortBy id
        
    let run =
        let timer = new System.Timers.Timer()
        timer.AutoReset <- true
        timer.Interval <- 5000 //5 sec
        timer.Enabled <- true
        let io = Resolver.value
        fun (getParam: System.Func<ClientParam array>) ->
            timer.Elapsed
            |> Observable.add (fun args ->
                let clientParams = getParam.Invoke()

                for clientParam in clientParams do
                    saveClient io clientParam 

                )
