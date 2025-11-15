namespace Tefin.Core.Interop

open System.Collections.Generic


[<AutoOpen>]
module ProjectTypes =
  
  type NamedPipeServerConfig() =
    member val PipeName = "" with get, set
    
  type ServiceMockConfig() =
    member val ServiceName = "" with get, set
    member val Port = 0u with get, set
    member val Desc = "" with get,set
    member val IsUsingNamedPipes = false with get,set
    member val NamedPipe = NamedPipeServerConfig() with get,set
        
  type NamedPipeClientConfig() =
    member val PipeName = "" with get, set
    member val Direction = "InOut" with get, set
    member val Options = [| "WriteThrough"; "Asynchronous" |]
    member val ImpersonationLevel = "Anonymous" with get,set
    
    (*
    direction: PipeDirection.InOut,
            options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
            impersonationLevel: TokenImpersonationLevel.Anonymous);
  *)
    
  type ClientTransportConfig =
  | NamedPipeClient of NamedPipeClientConfig
  | DefaultClient
  
  type ServerTransportConfig =
  | NamedPipeServer of NamedPipeServerConfig
  | DefaultServer
    
  //Use a class instead of an F# record to easily serialize to json
  type ClientConfig() =
    member val Name = "" with get, set
    member val ServiceName = "" with get, set
    member val Url = "" with get, set
    member val IsUsingSSL = false with get, set
    member val Jwt = "" with get, set
    member val Description = "" with get, set
    member val IsCertFromFile = false with get, set
    member val CertStoreLocation = "" with get, set
    member val CertThumbprint = "" with get, set
    member val CertFile = "" with get, set
    member val CertFilePassword = "" with get, set  
    member val IsUsingNamedPipes = false with get, set
    member val NamedPipe = NamedPipeClientConfig() with get, set

  type MethodGroup =
    { Name: string
      Path: string
      RequestFiles: string array }

    static member Empty() =
      { Name = ""
        Path = ""
        RequestFiles = Array.empty }
  type ServiceMockMethodGroup =
    { Name: string
      Path: string
      ScriptFile: string  }

    static member Empty() =
      { Name = ""
        Path = ""
        ScriptFile = "" }    

  type SubPath =
    { Code:string
      Collections:string
      Perf:string
      Tests:string
      Methods : string }
  type ClientGroup =
    { Name: string
      CodeFiles: string array
      ConfigFile: string
      Config: ClientConfig option
      Methods: MethodGroup array
      SubPath : SubPath
      Path: string }
    static member ConfigFilename = "config.json"
    static member Empty() =
      { Name = ""
        CodeFiles = Array.empty
        ConfigFile = ""
        Path = ""
        Methods = Array.empty
        SubPath = { Code = ""; Collections = ""; Methods = ""; Perf= ""; Tests = "" }
        Config = None }

  type ServiceMockSubPath =
    { Code:string      
      Methods : string }
  type ServiceMockGroup =
    { Name: string
      CodeFiles: string array
      ConfigFile: string
      Config: ServiceMockConfig option
      Methods: ServiceMockMethodGroup array
      SubFolders : ServiceMockSubPath
      Path: string }
    static member ConfigFilename = "mockConfig.json"
    static member Empty() =
      { Name = ""
        CodeFiles = Array.empty
        ConfigFile = ""
        Path = ""
        Methods = Array.empty
        SubFolders = { Code = ""; Methods = "" }
        Config = None }
  type Project =
    { Clients: ResizeArray<ClientGroup>
      Mocks : ResizeArray<ServiceMockGroup>
      ConfigFile: string
      Name: string
      Package: string
      Path: string }

    static member ProjectConfigFileName = "projectConfig.json" //TODO
    static member DefaultName = "_default"

    static member Empty() =
      { Clients = ResizeArray<ClientGroup>()
        Mocks = ResizeArray<ServiceMockGroup>()
        ConfigFile = ""
        Package = ""
        Name = ""
        Path = "" }

  type ClientSaveState =
    { Name: string
      OpenFiles: string array }
  type ServiceMockSaveState =
    { Name: string
      OpenScripts: string array }
    static member Empty() = { Name = ""; OpenScripts = Array.empty }

  type ProjectSaveState =
    { Package: string
      ClientState: ClientSaveState array
      MockState: ServiceMockSaveState array }

    static member FileName = "projectState.json"

    static member Empty(package) =
      { Package = package
        ClientState = Array.empty
        MockState = Array.empty }
