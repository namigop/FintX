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
    let loadClient (clientPath: string) =
        let configFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)
        let config = Instance.jsonDeserialize<ClientConfig> (File.ReadAllText configFile)

        let codePath = Path.Combine(clientPath, "code")
        let files = Directory.GetFiles(codePath, "*.*")

        { ClientGroup.Empty() with
            ConfigFile = configFile
            Config = Some config
            CodeFiles = files
            Name = config.Name
            Path = clientPath }

    let loadProject (projectPath: string) =
        let clientPaths =
            let files =
                Directory.GetFiles(projectPath, ClientGroup.ConfigFilename, SearchOption.AllDirectories)

            files |> Array.map (fun file -> Path.GetDirectoryName file)

        let name = Path.GetFileName projectPath
        let clients = clientPaths |> Array.map (fun path -> loadClient path)
        let config = Path.Combine(projectPath, Project.ProjectConfigFileName)

        { Project.Empty() with
            Name = name
            Clients = clients
            ConfigFile = config
            Path = projectPath }

    let updateClientConfig (io: IOResolver) (clientConfigFile: string) (clientConfig: ClientConfig) =
        task {
            let oldClientPath = Path.GetDirectoryName clientConfigFile
            let file, filePath  =
                let fileName = Path.GetFileName clientConfigFile
                let oldName = Path.GetDirectoryName clientConfigFile |> fun p -> Path.GetFileName p

                let currentName = clientConfig.Name
                let nameChanged = not (oldName = currentName)

                if nameChanged then
                    let newClientPath = Path.GetDirectoryName oldClientPath |> fun p -> Path.Combine(p, currentName)
                    Directory.Move(oldClientPath, newClientPath)

                    let newConfigFile = Path.Combine(newClientPath, fileName)
                    newConfigFile, newClientPath
                else
                    clientConfigFile, oldClientPath

            let json = Instance.jsonSerialize clientConfig
            do! io.File.WriteAllTextAsync file json

            let clientGroup = loadClient filePath
            GlobalHub.publish(MsgClientUpdated(clientGroup, filePath, oldClientPath))
        }

    let deleteClient (cfg:ClientGroup) (io:IOResolver) =
         io.Dir.Delete cfg.Path true //deletes everything
         io.Log.Info $"Deleted {cfg.Name}"            
        
    let addClient (io: IOResolver) (project: Project) clientName serviceName protoOrUrl description (csFiles: string array) =
        task {
            //1. Create the client folder
            let clientPath = Path.Combine(project.Path, clientName)
            io.Dir.CreateDirectory clientPath

            let config =  ClientConfig(Description = description,
                                       ServiceName = serviceName,
                                       Url = protoOrUrl,
                                       Name = clientName)

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