namespace Tefin.Grpc.Discovery

open System
open System.Collections.ObjectModel
open System.IO
open System.Threading.Tasks
open Tefin.Core
open Tefin.Core.Res
open Tefin.Grpc

type ServerDiscoverParameters =
  { Address: string
    ServiceName: string
    CustomClientName: string
    Description: string
    Config: ReadOnlyDictionary<string, string> }

module ServerReflectionDiscoveryClient =

  // type T(address, serviceName, customClientName, description, discover, validate) =
  //     interface IDiscoverClient with
  //         member val Description = address with get, set
  //         member val Locations = Array.empty with get, set
  //         member val Name = customClientName with get, set
  //         member x.Discover cfg = discover cfg x
  //         member x.Validate() = validate x



  let private validate (discoParams: ServerDiscoverParameters) =
    if String.IsNullOrWhiteSpace(discoParams.Address) then
      Ret.Error(failwith "Empty reflection service address!")
    else
      let address = discoParams.Address
      let isHttp = address.StartsWith("http") || address.StartsWith("https")

      if (not isHttp) then
        Ret.Error(failwith $"Address \"{address}\" should start with http or https")
      else
        Ok discoParams


  let generateSource (io: IOs) (discoParams: ServerDiscoverParameters) =
    task {
      let config = discoParams.Config
      let targetPath = Path.Combine(config["RootPath"], "temp")

      let! protoFilesRet =
        validate discoParams
        |> Task.FromResult
        |> mapTask (fun dParams ->
          task {
            let! protoFiles = GrpcReflectionClient.createProtoFile io dParams.Address dParams.ServiceName targetPath
            return protoFiles
          })
        |> mapTask (fun pr ->
          task {
            let protoDiscoParams: ProtoDiscoverParameters =
              { ProtoFiles = pr.ToArray()
                CustomClientName = discoParams.CustomClientName
                Description = discoParams.Description
                Config = config }

            let! filesResult = ProtoDiscoveryClient.generateSource io protoDiscoParams
            return filesResult
          })

      return protoFilesRet
    }
