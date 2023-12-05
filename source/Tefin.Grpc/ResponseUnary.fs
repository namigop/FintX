namespace Tefin.Grpc.Execution

open System
open System.Reflection
open Tefin.Core.Execution
open Tefin.Core.Interop
open System.Threading.Tasks
open Tefin.Core

type OkayUnaryResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ErrorUnaryResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ResponseUnary =
    | Okay of OkayUnaryResponse
    | Error of ErrorUnaryResponse

    member x.OkayOrFailed() =
        match x with
        | Okay v -> struct (true, v.Response, v.Context)
        | Error err -> struct (true, err.Response, err.Context)

module UnaryResponse =

    let awaitCall (methodInfo: MethodInfo) (resp2: obj) =
        task {
            let responseProperty = methodInfo.ReturnType.GetProperty("ResponseAsync")
            let task = responseProperty.GetValue(resp2) :?> Task
            do! task
            let resultProperty = task.GetType().GetProperty("Result")
            return resultProperty.GetValue(task)
        }

    let private wrapResponse (methodInfo: MethodInfo) (resp: obj) (isError: bool) =
        task {
            let wrapperType = ResponseUtils.emitUnaryResponse methodInfo isError
            let wrapperInst = Activator.CreateInstance(wrapperType)
            let prop = wrapperType.GetProperty("Response")
            prop.SetValue(wrapperInst, resp)
            return wrapperInst
        }

    let create (methodInfo: MethodInfo) (ctx: Context) =
        task {
            if ctx.Success then
                let! w = wrapResponse methodInfo (Res.getValue ctx.Response) false

                let t: OkayUnaryResponse =
                    { MethodInfo = methodInfo
                      Context = ctx
                      Response = w }

                return (Okay t)
            else
                let err = Res.getError ctx.Response

                let! w = wrapResponse methodInfo (ErrorResponse(Error = err.Message)) true

                let t: ErrorUnaryResponse =
                    { MethodInfo = methodInfo
                      Context = ctx
                      Response = w }

                return (Error t)
        }

    let createJson (resp: ResponseUnary) =
        match resp with
        | Okay k ->
            let isError = false
            wrapResponse k.MethodInfo k.Response isError
        | Error e ->
            let isError = true
            wrapResponse e.MethodInfo e.Response isError