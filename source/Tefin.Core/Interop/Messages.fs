namespace Tefin.Core.Interop



[<AutoOpen>]
module MessageProject =

  //Message published when a project is loaded
  type MsgProjectLoaded(proj: Project, folderPath: string) =
    inherit MsgBase($"Project@{folderPath}")
    member x.Project = proj
    member x.Path = folderPath

  type MsgClientUpdated(client: ClientGroup, folderPath: string, previousPath: string) =
    inherit MsgBase($"Client@{folderPath}")
    member x.Client = client
    member x.PreviousPath = previousPath
    member x.Path = folderPath
    
  type MsgServiceMockUpdated(client: ServiceMockGroup, folderPath: string, previousPath: string) =
    inherit MsgBase($"ServiceMock@{folderPath}")
    member x.Client = client
    member x.PreviousPath = previousPath
    member x.Path = folderPath
