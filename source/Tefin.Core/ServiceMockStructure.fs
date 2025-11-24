namespace Tefin.Core

open System.Reflection
open Tefin.Core.Infra.Actors
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
      let methodDirs = io.Dir.GetDirectories methodPath //"*.*" SearchOption.TopDirectoryOnly

      methodDirs
      |> Array.map (fun m ->
        let methodName = Path.GetFileName m
        let scriptFiles =
          io.Dir.GetFiles (m, "*" + Ext.mockScriptExt, SearchOption.AllDirectories)
          |> Array.filter (fun fp -> not <| fp.Contains(AutoSaveFolderName)) //ignore auto-saved files

        { ScriptFile = if scriptFiles.Length > 0 then scriptFiles[0] else "" 
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
    
    let addMethod (io:IOs) (methodsPath:string) (method:MethodInfo) =
      let path = Path.Combine(methodsPath, method.Name)
      io.Dir.CreateDirectory path
      //let scriptFile = Path.Combine(path, $"{method.Name}_script{Ext.mockScriptExt}")
      //io.File.WriteAllText scriptFile "<empty>"
      
    let addServiceMock
        (io: IOs)
        (project: Project)
        (serviceName:string)        
        description
        (csFiles: string array)
        (dll:string)
        (port:uint)        
        (serverTransport: ServerTransportConfig)
        (methods:MethodInfo array) =
          
        let mockPath = Path.Combine(project.Path, "mocks", Utils.makeValidFileName serviceName)
        io.Dir.CreateDirectory mockPath
      
        let mockConfig =
          match serverTransport with
          | NamedPipeServer pipe ->
              ServiceMockConfig( ServiceName = serviceName,
                                 Port = port,
                                 Desc = description,
                                 IsUsingNamedPipes = true,
                                 IsUsingUnixDomainSockets = false,
                                 NamedPipe = pipe)
          | UdsServer uds ->
              ServiceMockConfig( ServiceName = serviceName,
                                 Port = port,
                                 Desc = description,
                                 IsUsingNamedPipes = false,
                                 IsUsingUnixDomainSockets = true,
                                 UnixDomainSockets = uds)
          | DefaultServer ->
              ServiceMockConfig( ServiceName = serviceName,
                                 Port = port,
                                 Desc = description,
                                 IsUsingNamedPipes = false,
                                 IsUsingUnixDomainSockets = false)
                                     
                                            
        let mockConfigFile = Path.Combine(mockPath, ServiceMockGroup.ConfigFilename)
        let json = Instance.jsonSerialize mockConfig
        io.File.WriteAllText mockConfigFile json
        
        let codePath = Path.Combine(mockPath, "code")
        io.Dir.CreateDirectory codePath
        
        let dllName = Path.GetFileName dll
        let dlLTarget = Path.Combine(codePath, dllName)
        io.File.Copy(dll, dlLTarget, true)
                
        for m in methods do          
          addMethod io (getMethodsPath mockPath) m
        
        for source in csFiles do
            let name = Path.GetFileName source
            let target = Path.Combine(codePath, name)
            io.File.Copy(source, target, true)
    
    let deleteMock (io: IOs)  (mock: ServiceMockGroup) =
      io.Dir.Delete mock.Path true //deletes everything
      io.Log.Info $"Deleted {mock.Name}"
    
    let updateConfig (io:IOs) (mockConfigFile:string) (cfg:ServiceMockConfig) =
      task {
        
        let oldDirName = Path.GetDirectoryName mockConfigFile |> Path.GetFileName
        let oldMockPath = Path.GetDirectoryName mockConfigFile 
        let currentName = cfg.ServiceName
        let nameChanged = not (oldDirName = currentName)

        let cfgFile, mockPath =
          if nameChanged then
            let newMockPath = Path.GetDirectoryName oldMockPath |> fun p -> Path.Combine(p, currentName)
            io.Dir.Move oldMockPath newMockPath
            let fileName = Path.GetFileName mockConfigFile
            let newConfigFile = Path.Combine(newMockPath, fileName)
            newConfigFile, newMockPath
          else
            mockConfigFile, oldMockPath

        let json = Instance.jsonSerialize cfg
        do! io.File.WriteAllTextAsync cfgFile json        
        let grp = loadServiceMock io mockPath
        GlobalHub.publish (MsgServiceMockUpdated(grp, oldMockPath, mockPath))
      }
      
      
        
  

