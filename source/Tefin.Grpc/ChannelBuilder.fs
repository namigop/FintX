namespace Tefin.Grpc.Execution

open System
open System.IO
open System.IO.Pipes
open System.Net.Security
open System.Net.Sockets
open System.Security.Cryptography.X509Certificates
open System.Security.Principal
open System.Threading
open System.Threading.Tasks
open Grpc.Core
open Grpc.Net.Client
open Grpc.Net.Client.Configuration
open System.Net.Http
open Tefin.Core
open Tefin.Core.Interop

type StoreCert =
  { Thumbprint: string; Location: string }

type FileCert = { File: string; Password: string }

type Cert =
  | FromFile of FileCert
  | FromStore of StoreCert

type CallConfig =
  { Url: string
    IsUsingSSL: bool
    JWT: string
    X509Cert: Cert option
    IsUsingNamedPipes : bool
    NamedPipe : NamedPipeClientConfig
    IsUsingUnixDomainSockets : bool
    IsUsingHttp2 : bool
    IsUsingHttp3 : bool
    UnixDomainSocketConfig : UnixDomainSocketClientConfig
    Io: IOs }

  static member From  (cfg: ClientConfig) (io: IOs) (envFile:string)=
    let cert =
      if cfg.IsUsingSSL then
        if cfg.IsCertFromFile then
          Cert.FromFile
            { File = cfg.CertFile
              Password =  Utils.decrypt cfg.CertFilePassword (System.IO.Path.GetFileName cfg.CertFile )  }
          |> Some

        else
          Cert.FromStore
            { Thumbprint = cfg.CertThumbprint
              Location = cfg.CertStoreLocation }
          |> Some

      else
        None

    let tryExpand (url:string) =
      if url.Contains("{{") && url.Contains("}}") then       
        let tag = url.Substring(url.IndexOf("{{"), url.IndexOf("}}") - url.IndexOf("{{") + 2)
        (VarsStructure.getVarsFromFile io envFile).Variables
        |> Seq.tryFind (fun x -> x.Name = tag)
        |> function
           | Some x -> x.CurrentValue
           | None -> url         
      else
        url
    { Url = tryExpand (cfg.Url.Trim())
      IsUsingSSL = cfg.IsUsingSSL
      JWT = cfg.Jwt.Trim()
      X509Cert = cert
      IsUsingNamedPipes = cfg.IsUsingNamedPipes
      NamedPipe = cfg.NamedPipe
      IsUsingUnixDomainSockets = cfg.IsUsingUnixDomainSockets
      IsUsingHttp2 = cfg.IsUsingHttp2
      IsUsingHttp3 = cfg.IsUsingHttp3
      UnixDomainSocketConfig = cfg.UnixDomainSockets
      Io = io }

module ChannelBuilder =

  let private getCert (cert: Cert) =
    match cert with
    | FromFile fileCert -> CertUtils.createFromFile fileCert.File fileCert.Password       
    | FromStore storeCert ->
      let location = Enum.Parse(typeof<StoreLocation>, storeCert.Location) :?> StoreLocation
      CertUtils.findByThumbprint storeCert.Thumbprint location

  let getJWTCredentials (cfg: CallConfig) =
    let authInterceptor =
      AsyncAuthInterceptor(fun ctx metadata -> task { metadata.Add("Authorization", $"Bearer {cfg.JWT}") })

    let callCredentials = CallCredentials.FromInterceptor(authInterceptor)
    callCredentials

  let buildChannel (cfg: CallConfig) (httpclient: HttpClient) (defaultMethodConfig: MethodConfig) =
    let svcConfig = ServiceConfig()
    svcConfig.MethodConfigs.Add defaultMethodConfig

    let callCredentialsOpt =
      if not (String.IsNullOrWhiteSpace cfg.JWT) then
        let jwtCredentials = getJWTCredentials cfg
        Some jwtCredentials
      else
        None

    let channelCredentials =
      if cfg.Url.StartsWith("https://") then
        ChannelCredentials.SecureSsl
      else
        ChannelCredentials.Insecure

    let composedCredentials =
      match callCredentialsOpt with
      | Some c -> ChannelCredentials.Create(channelCredentials, c)
      | None -> channelCredentials

    let options =      
      if cfg.IsUsingHttp3 then
        GrpcChannelOptions(
          HttpClient = httpclient,
          Credentials = composedCredentials,
          UnsafeUseInsecureChannelCallCredentials = (callCredentialsOpt.IsSome && cfg.Url.StartsWith("http://")),
          ServiceConfig = svcConfig,
          HttpVersion = Version(3,0),
          HttpVersionPolicy = if cfg.IsUsingHttp2 then HttpVersionPolicy.RequestVersionOrLower else HttpVersionPolicy.RequestVersionExact)
      elif cfg.IsUsingHttp2  then
        GrpcChannelOptions(
          HttpClient = httpclient,
          Credentials = composedCredentials,
          UnsafeUseInsecureChannelCallCredentials = (callCredentialsOpt.IsSome && cfg.Url.StartsWith("http://")),
          ServiceConfig = svcConfig
        )
      else
        GrpcChannelOptions(HttpClient = httpclient)
    
    GrpcChannel.ForAddress(cfg.Url, options)
    

  let createSocketHandler (cfg : CallConfig) =
    let handler =
      new SocketsHttpHandler(
        EnableMultipleHttp2Connections = true,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60L),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30L),
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        SslOptions = SslClientAuthenticationOptions( ApplicationProtocols = ResizeArray<SslApplicationProtocol>())
      )
        
    if cfg.IsUsingHttp2  then
      handler.SslOptions.ApplicationProtocols.Add(SslApplicationProtocol.Http2)    
    if cfg.IsUsingHttp3  then
      handler.SslOptions.ApplicationProtocols.Add(SslApplicationProtocol.Http3)
    handler
    
  let private createSecureSocketHandler (cfg: CallConfig) (defaultMethodConfig: MethodConfig) =
    let x509 = getCert cfg.X509Cert.Value
    let ignoreSslChecks = RemoteCertificateValidationCallback(fun a b c d -> true)

    let handler = createSocketHandler cfg
    handler.SslOptions <- SslClientAuthenticationOptions(RemoteCertificateValidationCallback = ignoreSslChecks,
                                                         ApplicationProtocols = ResizeArray<SslApplicationProtocol>(),
                                                         ClientCertificates =   X509Certificate2Collection())    
    if cfg.IsUsingHttp2  then
      handler.SslOptions.ApplicationProtocols.Add(SslApplicationProtocol.Http2)
    if cfg.IsUsingHttp3  then
      handler.SslOptions.ApplicationProtocols.Add(SslApplicationProtocol.Http3)
     
    let _ = handler.SslOptions.ClientCertificates.Add(x509)
    handler
 
  let private createChannel (cfg: CallConfig) (defaultMethodConfig: MethodConfig) (handler:SocketsHttpHandler) =     
    let httpclient = new HttpClient(handler, Timeout = Timeout.InfiniteTimeSpan)
    buildChannel cfg httpclient defaultMethodConfig
    
  let createNamedPipeChannel (cfg: CallConfig)  (namedPipe:NamedPipeClientConfig) (handler:SocketsHttpHandler) (defaultMethodConfig: MethodConfig) =
      let pipeDirection =
         let ok, v = Enum.TryParse<PipeDirection>(namedPipe.Direction)
         if ok then v else PipeDirection.InOut
      let impersonation =
         let ok, v = Enum.TryParse<TokenImpersonationLevel>(namedPipe.ImpersonationLevel)
         if ok then v else TokenImpersonationLevel.Anonymous
      let options =
        let mutable pipeOptions = PipeOptions.None
        if Array.contains "WriteThrough" namedPipe.Options then
          pipeOptions <- pipeOptions ||| PipeOptions.WriteThrough
        if Array.contains "Asynchronous" namedPipe.Options then
          pipeOptions <- pipeOptions ||| PipeOptions.Asynchronous
        if Array.contains "CurrentUserOnly" namedPipe.Options then
          pipeOptions <- pipeOptions ||| PipeOptions.CurrentUserOnly
        if Array.contains "FirstPipeInstance" namedPipe.Options then
          pipeOptions <- pipeOptions ||| PipeOptions.FirstPipeInstance
          
        pipeOptions
        
      
      let createNamedPipeStream  (ctx: SocketsHttpConnectionContext) (token:CancellationToken)  =      
          task {
            let clientStream = new NamedPipeClientStream(
              ".", //named pipes are supported only on the local machine
              namedPipe.PipeName,
              pipeDirection,
              options,
              impersonation)
            
            try
                do! clientStream.ConnectAsync(token).ConfigureAwait(false)              
            with exc ->
                clientStream.Dispose()
                raise exc              
            return clientStream :> Stream
          }
          |> fun t -> ValueTask.FromResult(t.Result)                
      
      let callback = Func<SocketsHttpConnectionContext, CancellationToken,ValueTask<Stream>>(createNamedPipeStream)
      handler.ConnectCallback <- callback
      
      let svcConfig = ServiceConfig()
      svcConfig.MethodConfigs.Add defaultMethodConfig
      
      let channelOptions =      
        if cfg.IsUsingHttp3 && cfg.IsUsingHttp2 then
          GrpcChannelOptions(
            ServiceConfig = svcConfig,
            HttpHandler = handler,
            HttpVersion = Version(3,0),
            HttpVersionPolicy =  HttpVersionPolicy.RequestVersionOrLower)
        elif cfg.IsUsingHttp2 then
          GrpcChannelOptions(
            ServiceConfig = svcConfig,
            HttpHandler = handler,
            HttpVersion =  Version(2,0),
            HttpVersionPolicy =  HttpVersionPolicy.RequestVersionExact)
        elif cfg.IsUsingHttp3 then
          GrpcChannelOptions(
            ServiceConfig = svcConfig,
            HttpHandler = handler,
            HttpVersion =  Version(3,0),
            HttpVersionPolicy =  HttpVersionPolicy.RequestVersionExact)
        else
          GrpcChannelOptions(HttpHandler = handler, ServiceConfig = svcConfig)
          
      let channel = GrpcChannel.ForAddress(cfg.Url, channelOptions)     
      channel
  
  let createUdsChannel (cfg:CallConfig) (uds:UnixDomainSocketClientConfig) (handler:SocketsHttpHandler) (defaultMethodConfig: MethodConfig) =
      let protocolType =
         let ok, v = Enum.TryParse<ProtocolType>(uds.ProtocolType)
         if ok then v else ProtocolType.Unspecified
      let socketType =
         let ok, v = Enum.TryParse<SocketType>(uds.SocketType)
         if ok then v else SocketType.Stream
      
      let createUdsStream  (ctx: SocketsHttpConnectionContext) (token:CancellationToken)  =      
          task {
            let socketPath = Path.Combine(Path.GetTempPath(), uds.SocketFilePath)
            let  udsEndPoint = UnixDomainSocketEndPoint(socketPath)            
            let socket = new Socket(AddressFamily.Unix, socketType, protocolType)

            try
                do! socket.ConnectAsync(udsEndPoint, token).ConfigureAwait(false)       
            with exc ->
                socket.Dispose()
                raise exc              

            return new NetworkStream(socket, true) :> Stream
          }
          |> fun t -> ValueTask.FromResult(t.Result)                
      
        
      let callback = Func<SocketsHttpConnectionContext, CancellationToken,ValueTask<Stream>>(createUdsStream)
      handler.ConnectCallback <- callback
        
      let svcConfig = ServiceConfig()
      svcConfig.MethodConfigs.Add defaultMethodConfig
      
      let channelOptions =      
        if cfg.IsUsingHttp3 && cfg.IsUsingHttp2 then
          GrpcChannelOptions(
            ServiceConfig = svcConfig,
            HttpHandler = handler,
            HttpVersion = Version(3,0),
            HttpVersionPolicy =  HttpVersionPolicy.RequestVersionOrHigher)
        elif cfg.IsUsingHttp2 then
          GrpcChannelOptions(
            ServiceConfig = svcConfig,
            HttpHandler = handler,
            HttpVersion =  Version(2,0),
            HttpVersionPolicy =  HttpVersionPolicy.RequestVersionExact)
        elif cfg.IsUsingHttp3 then
          GrpcChannelOptions(
            ServiceConfig = svcConfig,
            HttpHandler = handler,
            HttpVersion =  Version(3,0),
            HttpVersionPolicy =  HttpVersionPolicy.RequestVersionExact)
        else
          GrpcChannelOptions(HttpHandler = handler, ServiceConfig = svcConfig)
          
      let channel = GrpcChannel.ForAddress(cfg.Url, channelOptions)     
      channel
      
  let createGrpcChannel (cfg: CallConfig) =
    if cfg.IsUsingHttp2 = false && cfg.IsUsingHttp3 = false then
      raise (ArgumentException("At least one of Http2 or Http3 must be enabled"))
    
    let defaultMethodConfig =
      let m = MethodConfig()
      m.Names.Add(MethodName.Default)
      m.RetryPolicy <-
        RetryPolicy(
          MaxAttempts = 5,
          InitialBackoff = TimeSpan.FromSeconds 1L,
          MaxBackoff = TimeSpan.FromSeconds 5L,
          BackoffMultiplier = 1.5
        )

      m.RetryPolicy.RetryableStatusCodes.Add(StatusCode.Unavailable)
      m
    
    let socketHandler =
      if cfg.IsUsingSSL && cfg.Url.StartsWith "https://" then
        createSecureSocketHandler cfg defaultMethodConfig
      else
        createSocketHandler cfg
        
    if cfg.IsUsingNamedPipes then
      createNamedPipeChannel cfg cfg.NamedPipe  socketHandler defaultMethodConfig
    elif cfg.IsUsingUnixDomainSockets then
      createUdsChannel cfg cfg.UnixDomainSocketConfig  socketHandler defaultMethodConfig
    else
      createChannel cfg defaultMethodConfig socketHandler
