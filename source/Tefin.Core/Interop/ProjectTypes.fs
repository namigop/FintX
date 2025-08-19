namespace Tefin.Core.Interop


[<AutoOpen>]
module ProjectTypes =

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

  type MethodGroup =
    { Name: string
      Path: string
      RequestFiles: string array }

    static member Empty() =
      { Name = ""
        Path = ""
        RequestFiles = Array.empty }

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

  type Project =
    { Clients: ClientGroup array
      ConfigFile: string
      Name: string
      Package: string
      Path: string }

    static member ProjectConfigFileName = "projectConfig.json" //TODO
    static member DefaultName = "_default"

    static member Empty() =
      { Clients = Array.empty
        ConfigFile = ""
        Package = ""
        Name = ""
        Path = "" }

  type ClientSaveState =
    { Name: string
      OpenFiles: string array }

  type ProjectSaveState =
    { Package: string
      ClientState: ClientSaveState array }

    static member FileName = "projectState.json"

    static member Empty(package) =
      { Package = package
        ClientState = Array.empty }
