namespace Tefin.Grpc.Execution

open System.Reflection
open System.Threading.Tasks
open Tefin.Core
open Tefin.Core.Execution
open Tefin.Core.Interop

module CallServerStreaming =

    let callError = CallError()

    let runSteps (io: IOResolver) (methodInfo: MethodInfo) (mParams: obj array) (callConfig: CallConfig) =
        task {
            let start (ctx: Context) =
                //TODO: initialization code
                Task.FromResult { ctx with Io = Some io }

            let invoke (ctx: Context) =
                task {
                    ctx.Io.Value.Log.Info $"Invoking {methodInfo.Name} @ {callConfig.Url}"

                    try
                        callError.Clear()
                        let! resp = MethodInvoker.invoke methodInfo mParams callConfig callError.Receive

                        if callError.Failed then
                            return
                                { ctx with
                                    Response = Res.ok resp.Value
                                    Error = callError.Exception }
                        else
                            return
                                { ctx with
                                    Response = Res.ok resp.Value }

                    //return { ctx with Response = Res.ok resp.Value }
                    with exc ->
                        return { ctx with Response = Res.failed exc }
                }

            let stop (ctx: Context) = Task.FromResult ctx
            let execContext = CallPipeline.start ()

            return!
                CallPipeline.exec
                    [| { Name = "start"; Run = start }
                       { Name = "invoke"; Run = invoke }
                       { Name = "stop"; Run = stop } |]
                    execContext
        }

    let run (io: IOResolver) (methodInfo: MethodInfo) (mParams: obj array) (cfg: ClientConfig) =
        task {
            let callConfig = CallConfig.From cfg io
            let! ctx = runSteps io methodInfo mParams callConfig
            let resp = ServerStreamingResponse.create methodInfo ctx
            return struct (ctx.Success, resp)
        }
