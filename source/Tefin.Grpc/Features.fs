namespace Tefin.Grpc

open Tefin.Core
open Tefin.Grpc
open Tefin.Core.Res
open System.Threading.Tasks
open System.Text.RegularExpressions

//open Sto

module Features =
    let private io = Resolver.value
    let discover (discoParams : DiscoverParameters) = task {

    (*
     private void PopulateServiceNamesFromProto() {
        this.DiscoveredServices.Clear();
        var regex = @"service\s+(?<ServiceName>\w+)\s+";
        foreach (var l in File.ReadAllLines(this.ProtoFilesOrUrl)) {
            var m = Regex.Match(l, regex);
            if (m.Success) {
                this.DiscoveredServices.Add(m.Groups["ServiceName"].Value);
            }
        }
         

        if (this.DiscoveredServices.Count > 0) {
            this.SelectedDiscoveredService = this.DiscoveredServices[0];
        }
    }

    *)

        if (discoParams.ProtoFiles.Length > 0) then
            let regex = @"service\s+(?<ServiceName>\w+)\s+";
            let lines = io.File.ReadAllLines discoParams.ProtoFiles[0]
            let services =  
                lines 
                |> Array.map (fun l -> Regex.Match(l, regex)) 
                |> Array.filter (fun g -> g.Success)
                |> Array.map (fun m -> m.Groups["ServiceName"].Value)
            
            return (Ret.Ok services)

         else
            let! services = GrpcReflectionClient.getServices io discoParams.ReflectionUri.AbsoluteUri
            return services
    }

    let generateSourceFiles (compileParams:CompileParameters) = task {
        let grpcParams = {compileParams with Config = GrpcPackage.grpcConfigValues }
        let! sourceFiles = ServiceClient.generateSourceFiles io grpcParams
        return sourceFiles
    }

    let compile sourceFiles (compileParams:CompileParameters) = task {
        let grpcParams = {compileParams with Config = GrpcPackage.grpcConfigValues }
        let! c =
            Task.FromResult(sourceFiles)
            |> mapTask (fun csFiles -> task {
                let! compile = ServiceClient.compile io { grpcParams with CsFiles =  csFiles }
                return compile
            })
        return c
        }