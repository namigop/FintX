namespace Tefin.Grpc

open System.Text
open System.Text.RegularExpressions
open System.Threading.Tasks
open Tefin.Core

module ServiceMock =  
    let private unaryMethodRegex = @"public\s+virtual.*Task<(?<ResponseType>\S+)>\s+(?<MethodName>\w+)\(\S+\s\w+,\s\S+\s\w+\)"
    //public virtual global::System.Threading.Tasks.Task<global::Grpc.Testing.SimpleResponse> StreamingFromClient(grpc::IAsyncStreamReader<global::Grpc.Testing.SimpleRequest> requestStream, grpc::ServerCallContext context)
    let private clientStreamMethodRegex = @"public\s+virtual.*Task<(?<ResponseType>\S+)>\s+(?<MethodName>\w+)\(.*IAsyncStreamReader.*\)"
    //public virtual global::System.Threading.Tasks.Task StreamingFromServer(global::Grpc.Testing.SimpleRequest request, grpc::IServerStreamWriter<global::Grpc.Testing.SimpleResponse> responseStream, grpc::ServerCallContext context)
    let private serverStreamMethodRegex = @"public\s+virtual.*Task\s+(?<MethodName>\w+)\(.*IServerStreamWriter.*\)"
    //public virtual global::System.Threading.Tasks.Task StreamingCall(grpc::IAsyncStreamReader<global::Grpc.Testing.SimpleRequest> requestStream, grpc::IServerStreamWriter<global::Grpc.Testing.SimpleResponse> responseStream, grpc::ServerCallContext context)
    let private duplexMethodRegex = @"public\s+virtual.*Task\s+(?<MethodName>\w+)\(.*IAsyncStreamReader.*IServerStreamWriter.*\)"
    let private methodSigRegex = @"public\s+virtual.+Task.*\(.*\)"
    let private serviceBaseRegex = @"public\s+abstract\s+partial\s+class\s+(?<ServiceName>\S+)Base"
    let private nsRegex = @"namespace\s(?<Namespace>\S+)\s+.*"
    
    let private constr = """
      public string ServiceName { get; private set;}
      public {{SERVICE_NAME}}(string serviceName)
      {
          this.ServiceName = serviceName;
      }

"""
    let private unaryTemplate =
        """            
        var resp = await global::Tefin.Features.Scripting.ServerHandler.RunUnary(this.ServiceName, "{{METHOD_NAME}}", request, context);
        return ({{RESPONSE_TYPE}})resp;
        """
    let private clientStreamTemplate =
        """            
        var resp = await global::Tefin.Features.Scripting.ServerHandler.RunClientStream(this.ServiceName, "{{METHOD_NAME}}", requestStream, context);
        return ({{RESPONSE_TYPE}})resp;
        """
    let private serverStreamTemplate =
        """            
        var task = global::Tefin.Features.ServerHandler.Scripting.RunServerStream(this.ServiceName, "{{METHOD_NAME}}", request, responseStream, context);
        await task;
        """
    let private duplexTemplate =
        """            
        var task = global::Tefin.Features.ServerHandler.Scripting.RunDuplex(this.ServiceName, "{{METHOD_NAME}}", requestStream, responseStream, context);
        await task;
        """
            
    let containsServiceBase (io:IOs) (csFile:string) =        
        let mutable found = false
        for l in io.File.ReadAllLines csFile do
            if (Regex.IsMatch(l, serviceBaseRegex)) then
              found <- true
        found
    
    let private getTemplate (l:string) serviceName =
        let isDuplex (line:string) = line.Contains("IAsyncStreamReader") && line.Contains("IServerStreamWriter")
        let isServerStreaming (line:string) = line.Contains("IServerStreamWriter")
        let isClientStreaming (line:string) = line.Contains("IAsyncStreamReader")
        
        let template, methodName =
            if (isDuplex l) then
                let um = Regex.Match(l, duplexMethodRegex)
                duplexTemplate, um.Groups["MethodName"].Value                                                                 
            elif (isServerStreaming l) then
                let um = Regex.Match(l, serverStreamMethodRegex)                                                        
                serverStreamTemplate, um.Groups["MethodName"].Value                                
            elif (isClientStreaming l) then
                let um = Regex.Match(l, clientStreamMethodRegex)
                let methodName = um.Groups["MethodName"].Value                          
                let responseType = um.Groups["ResponseType"].Value
                clientStreamTemplate.Replace("{{RESPONSE_TYPE}}", responseType), methodName
            else
                let um = Regex.Match(l, unaryMethodRegex)
                let methodName = um.Groups["MethodName"].Value                            
                let responseType = um.Groups["ResponseType"].Value
                unaryTemplate.Replace("{{RESPONSE_TYPE}}", responseType), methodName
        template
            .Replace("{{SERVICE_NAME}}", serviceName)
            .Replace("{{METHOD_NAME}}", methodName)           
                
    let genService (csFile:string) =                
        let mutable braceCounter = 0
        let mutable serviceName = ""
        let mutable canCopy = false
        let mutable lineNumber = 0        
        let mutable methodLine = ""
        let mutable canWriteConstructor = true
              
        let lines = File.fileIO.ReadAllLines csFile        
        let sb = StringBuilder()        
        let mutable endPos = 0
        for l in lines do
            lineNumber <- lineNumber + 1
           
            if l.Contains('{') then
                braceCounter <- braceCounter + 1
            if l.Contains('}') then
                braceCounter <- braceCounter - 1
          
            if canCopy && (braceCounter = 0) then
                canCopy <- false
                ignore (sb.AppendLine "}")
                endPos <- lineNumber
                
            let m = Regex.Match(l, serviceBaseRegex)
            if  m.Success then
                braceCounter <- 0                
                let baseName = m.Groups["ServiceName"].Value
                serviceName <- "FintXImpl" + baseName + "Service"
                sb.AppendLine($"public class {serviceName} : {baseName}Base") |> ignore
                canCopy <- true
            else
                if (canCopy) then                    
                    let mr = Regex.Match(l, methodSigRegex)
                    if (mr.Success) then
                        methodLine <- l
                        let newSign = l.Replace("public virtual", "public override async")
                        ignore (sb.AppendLine newSign)    
                    else if l.Contains("StatusCode.Unimplemented") then                        
                        ignore (sb.AppendLine (getTemplate methodLine serviceName) )
                    else
                        ignore (sb.AppendLine l)
                                                
                    if canWriteConstructor && (braceCounter = 1) then
                        canWriteConstructor <- false
                        sb.AppendLine() |> ignore
                        sb.AppendLine (constr.Replace("{{SERVICE_NAME}}", serviceName))
                        |> ignore
                    

        (serviceName, sb.ToString(), endPos)
    
    let insertService (io:IOs) (csFile:string) =
        let lines = io.File.ReadAllLines csFile
        let serviceName, code, lineNumber = genService csFile
        
        match lines |> Array.tryFind (fun l -> l.Contains(serviceName)) with
        | None ->        
            let sb = StringBuilder()
            let mutable ln = 0
            for l in lines do
                ln <- ln + 1
                ignore(sb.AppendLine l)
                if ln = lineNumber then
                    sb.AppendLine().AppendLine(code).AppendLine()
                    |> ignore            
            io.File.WriteAllTextAsync csFile (sb.ToString())
        | Some _ -> Task.CompletedTask
        
            
    

