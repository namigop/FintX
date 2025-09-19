namespace Tefin.Core

open Tefin.Core.Interop
open System.IO
open System

module ServiceMockStructure =
    
    let AutoSaveFolderName = "_autoSave"
    let getMethodsPath (mockPath: string) = Path.Combine(mockPath, "methods")

    let getMethodPath (mockPath: string) (methodName: string) =
      getMethodsPath mockPath |> fun m -> Path.Combine(m, methodName)

    let getAutoSavePath (clientPath: string) (methodName: string) =
      getMethodPath clientPath methodName
      |> fun m -> Path.Combine(m, AutoSaveFolderName)
    
    let loadMethods (io:IOs) (mockPath: string) =
      let methodPath = getMethodsPath mockPath
      io.Dir.CreateDirectory methodPath
      let methodDirs = io.Dir.GetDirectories methodPath //"*.*" SearchOption.TopDirectoryOnly)

      methodDirs
      |> Array.map (fun m ->
        let methodName = Path.GetFileName m

        let scriptFiles =
          io.Dir.GetFiles (m, "*" + Ext.mockScriptExt, SearchOption.AllDirectories)
          |> Array.filter (fun fp -> not <| fp.Contains(AutoSaveFolderName)) //ignore auto-saved files

        { RequestFiles = scriptFiles
          Name = methodName
          Path = m })
    
    let loadServiceMock (io:IOs) (mockPath: string) : ServiceMockGroup =
      let configFile = Path.Combine(mockPath, ServiceMockGroup.ConfigFilename)
      let config = Instance.jsonDeserialize<ServiceMockConfig> (io.File.ReadAllText configFile)

      let codePath = Path.Combine(mockPath, "code")
      let files =
        io.Dir.GetFiles (codePath, "*.cs", SearchOption.TopDirectoryOnly)
        |> Array.append (io.Dir.GetFiles (codePath, "*.dll", SearchOption.TopDirectoryOnly))
      
      { ConfigFile = configFile
        Name = config.ServiceName
        Config = Some config
        CodeFiles = files
        Methods = loadMethods io mockPath 
        SubFolders = { Code = codePath; Methods = Path.Combine(mockPath, "methods") }        
        Path = mockPath }
    
    let addMock (io: IOs) (project: Project) (serviceName:string) (protoOrUrl:string) description (csFiles: string array) (dll:string) =        
        let mockPath = Path.Combine(project.Path, "mocks", Utils.makeValidFileName serviceName)
        io.Dir.CreateDirectory mockPath
        
        let mockConfig = ServiceMockConfig( ServiceName = serviceName)
        let mockConfigFile = Path.Combine(mockPath, ServiceMockGroup.ConfigFilename)
        let json = Instance.jsonSerialize mockConfig
        io.File.WriteAllText mockConfigFile json
        
        let codePath = Path.Combine(mockPath, "code")
        io.Dir.CreateDirectory codePath
        
        let dllName = Path.GetFileName dll
        let dlLTarget = Path.Combine(codePath, dllName)
        io.File.Copy(dll, dlLTarget, true)
        
        for source in csFiles do
            let name = Path.GetFileName source
            let target = Path.Combine(codePath, name)
            io.File.Copy(source, target, true)
        
        
  

