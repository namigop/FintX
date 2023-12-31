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
    let getMethodPath (clientPath: string) = Path.Combine(clientPath, "methods")

    let loadMethods (clientPath: string) =
        let methodPath = getMethodPath clientPath
        Directory.CreateDirectory methodPath
        let methodDirs = Directory.GetDirectories(methodPath, "*.*", SearchOption.TopDirectoryOnly)

        methodDirs
        |> Array.map (fun m ->
            let methodName = Path.GetFileName m

            let requestFiles =
                Directory.GetFiles(m, "*" + Ext.requestFileExt, SearchOption.AllDirectories)

            {   RequestFiles = requestFiles
                Name = methodName
                Path = m }
        )

    let saveRequestFile (clientGroup: ClientGroup) (methodName: string) (reqFileName: string) (reqFileJson: string) =
        let dir = Path.Combine(clientGroup.Path, methodName)
        let _ = Directory.CreateDirectory dir
        File.WriteAllText(Path.Combine(dir, reqFileName), reqFileJson)

    let loadClient (clientPath: string) =
        let configFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)
        let config = Instance.jsonDeserialize<ClientConfig> (File.ReadAllText configFile)

        let codePath = Path.Combine(clientPath, "code")
        let files = Directory.GetFiles(codePath, "*.*")

        { ConfigFile = configFile
          Config = Some config
          CodeFiles = files
          Methods = loadMethods clientPath
          Name = config.Name
          Path = clientPath }

    let loadProject (projectPath: string) =
        let clientPaths =
            let files =
                Directory.GetFiles(projectPath, ClientGroup.ConfigFilename, SearchOption.AllDirectories)

            files |> Array.map (fun file -> Path.GetDirectoryName file)

        let projectName = Path.GetFileName projectPath
        let clients = clientPaths |> Array.map loadClient
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

            let clientGroup = loadClient filePath
            GlobalHub.publish (MsgClientUpdated(clientGroup, filePath, oldClientPath))
        }

    let deleteClient (cfg: ClientGroup) (io: IOResolver) =
        io.Dir.Delete cfg.Path true //deletes everything
        io.Log.Info $"Deleted {cfg.Name}"

    let addClient
        (io: IOResolver)
        (project: Project)
        clientName
        serviceName
        protoOrUrl
        description
        (csFiles: string array)
        =
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
