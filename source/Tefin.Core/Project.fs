namespace Tefin.Core

open Tefin.Core.Infra.Actors
open Tefin.Core.Interop
open System.IO

type AddClientRequest =
    { Name: string
      ReflectionServiceUrl: string
      //CsFiles
      ProtoFiles: string array
      Desc: string }

module Project =

    let autoSaveFolderName = "_autoSave"
    let getMethodsPath (clientPath: string) = Path.Combine(clientPath, "methods")

    let getMethodPath (clientPath: string) (methodName: string) =
        getMethodsPath clientPath |> fun m -> Path.Combine(m, methodName)

    let getAutoSavePath (clientPath: string) (methodName: string) =
        getMethodPath clientPath methodName
        |> fun m -> Path.Combine(m, autoSaveFolderName)

    let loadMethods (io: IOResolver) (clientPath: string) =
        let methodPath = getMethodsPath clientPath
        io.Dir.CreateDirectory methodPath
        let methodDirs = io.Dir.GetDirectories methodPath //"*.*" SearchOption.TopDirectoryOnly)

        methodDirs
        |> Array.map (fun m ->
            let methodName = Path.GetFileName m

            let requestFiles =
                io.Dir.GetFiles(m, "*" + Ext.requestFileExt, SearchOption.AllDirectories)
                |> Array.filter (fun fp -> not <| fp.Contains(autoSaveFolderName)) //ignore auto-saved files


            { RequestFiles = requestFiles
              Name = methodName
              Path = m })

    let loadClient (io: IOResolver) (clientPath: string) =
        let configFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)
        let config = Instance.jsonDeserialize<ClientConfig> (io.File.ReadAllText configFile)

        let codePath = Path.Combine(clientPath, "code")
        let files = Directory.GetFiles(codePath, "*.*")

        { ConfigFile = configFile
          Config = Some config
          CodeFiles = files
          Methods = loadMethods io clientPath
          Name = config.Name
          Path = clientPath }

    let loadProject (io: IOResolver) (projectPath: string) =
        let clientPaths =
            let files =
                Directory.GetFiles(projectPath, ClientGroup.ConfigFilename, SearchOption.AllDirectories)

            files |> Array.map (fun file -> Path.GetDirectoryName file)

        let projectName = Path.GetFileName projectPath
        let clients = clientPaths |> Array.map (fun path -> loadClient io path)
        let config = Path.Combine(projectPath, Project.ProjectConfigFileName)

        { Name = projectName
          Clients = clients
          ConfigFile = config
          Path = projectPath }

    let updateClientConfig (io: IOResolver) (clientConfigFile: string) (clientConfig: ClientConfig) =
        task {
            let oldClientPath = Path.GetDirectoryName clientConfigFile

            let file, filePath =
                let fileName = Path.GetFileName clientConfigFile
                let oldName = Path.GetDirectoryName clientConfigFile |> Path.GetFileName

                let currentName = clientConfig.Name
                let nameChanged = not (oldName = currentName)

                if nameChanged then
                    let newClientPath =
                        Path.GetDirectoryName oldClientPath |> fun p -> Path.Combine(p, currentName)

                    Directory.Move(oldClientPath, newClientPath)

                    let newConfigFile = Path.Combine(newClientPath, fileName)
                    newConfigFile, newClientPath
                else
                    clientConfigFile, oldClientPath

            let json = Instance.jsonSerialize clientConfig
            do! io.File.WriteAllTextAsync file json

            let clientGroup = loadClient io filePath
            GlobalHub.publish (MsgClientUpdated(clientGroup, filePath, oldClientPath))
        }

    let deleteClient (cfg: ClientGroup) (io: IOResolver) =
        io.Dir.Delete cfg.Path true //deletes everything
        io.Log.Info $"Deleted {cfg.Name}"

    let addClient (io: IOResolver) (project: Project) clientName serviceName protoOrUrl description (csFiles: string array) =
        task {
            //1. Create the client folder
            let clientPath = Path.Combine(project.Path, clientName)
            io.Dir.CreateDirectory clientPath

            let config =
                ClientConfig(Description = description, ServiceName = serviceName, Url = protoOrUrl, Name = clientName)

            let clientConfigFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)

            //2. Copy the C# files
            let codePath = Path.Combine(clientPath, "code")
            io.Dir.CreateDirectory codePath

            for source in csFiles do
                let name = Path.GetFileName source
                let target = Path.Combine(codePath, name)
                io.File.Copy(source, target, true)

            //3. Save the client config
            do! updateClientConfig io clientConfigFile config
        }
