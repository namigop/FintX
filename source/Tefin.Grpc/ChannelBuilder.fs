namespace Tefin.Grpc.Execution

open System
open System.Net.Security
open System.Security.Cryptography.X509Certificates
open System.Threading
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
    X509Cert: Cert option //todo
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

    GrpcChannel.ForAddress(
      cfg.Url,
      GrpcChannelOptions(
        HttpClient = httpclient,
        Credentials = composedCredentials,
        UnsafeUseInsecureChannelCallCredentials = (callCredentialsOpt.IsSome && cfg.Url.StartsWith("http://")),
        ServiceConfig = svcConfig
      )
    )

  let private createSecureChannel (cfg: CallConfig) (defaultMethodConfig: MethodConfig) =
    let x509 = getCert cfg.X509Cert.Value
    let ignoreSslChecks = RemoteCertificateValidationCallback(fun a b c d -> true)

    let handler =
      new SocketsHttpHandler(
        EnableMultipleHttp2Connections = true,
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60L),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30L),
        SslOptions = SslClientAuthenticationOptions(RemoteCertificateValidationCallback = ignoreSslChecks,
                                                    ClientCertificates =   new X509Certificate2Collection())
      )

    let _ = handler.SslOptions.ClientCertificates.Add(x509)
    let httpclient = new HttpClient(handler, Timeout = Timeout.InfiniteTimeSpan)
    buildChannel cfg httpclient defaultMethodConfig

  let private createInsecureChannel (cfg: CallConfig) (defaultMethodConfig: MethodConfig) =
    let handler =
      new SocketsHttpHandler(
        EnableMultipleHttp2Connections = true,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60L),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30L),
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
      )

    let httpclient = new HttpClient(handler, Timeout = Timeout.InfiniteTimeSpan)
    buildChannel cfg httpclient defaultMethodConfig

  let createGrpcChannel (cfg: CallConfig) =
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

    if cfg.IsUsingSSL && cfg.Url.StartsWith "https://" then
      createSecureChannel cfg defaultMethodConfig
    else
      createInsecureChannel cfg defaultMethodConfig
