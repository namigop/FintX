namespace Tefin.Core.Execution

open System
open System.Collections.Generic
open System.Diagnostics
open System.Threading.Tasks
open Tefin.Core

type Context =
  { StartTime: DateTime
    Steps: (string * TimeSpan) list
    Elapsed: TimeSpan option
    Error: Exception
    Response: Ret<obj>
    Io: IOResolver option
    Variable: Dictionary<string, obj> }

  member this.Success = (this.Error = null) && Res.isOk this.Response

  member this.GetError() =
    if (this.Error = null) then
      Res.getError this.Response
    else
      this.Error

type Step =
  { Name: string
    Run: Context -> Task<Context> }

module CallPipeline =

  let start () =
    { StartTime = DateTime.Now
      Steps = List.Empty
      Elapsed = None
      // Success = false
      Error = null
      Response = Res.failedWith "not started"
      Io = None
      Variable = Dictionary<string, obj>() }

  let exec (steps: Step array) (execContext: Context) =
    task {

      //Execute 1 step
      let execStep (step: Step) (ctx: Context) =
        task {
          let sw = Stopwatch.StartNew()

          try
            let! ctx2 = step.Run(ctx)
            sw.Stop()

            return
              { ctx2 with
                  Steps = [ step.Name, sw.Elapsed ] |> List.append ctx.Steps }
          with exc ->
            sw.Stop()

            return
              { ctx with
                  Response = Res.failed exc
                  //  Success = false
                  Steps = [ step.Name, sw.Elapsed ] |> List.append ctx.Steps }
        }

      //execute all steps
      // let rec run (steps:Step array) pos ctx = task {
      //     if (pos = steps.Length) then
      //         //last step
      //         let sum = ctx.Steps |> List.sumBy (fun (_, ts) -> ts.TotalMilliseconds)
      //         let ok = Res.isOk ctx.Response
      //         if not ok then
      //             ctx.Io.Value.Log.Error $"{Res.getError ctx.Response}"
      //
      //         return { ctx with Elapsed = Some (TimeSpan.FromMilliseconds sum)
      //                           Success = Res.isOk ctx.Response  }
      //     else
      //         let! updatedContext = execStep (steps[pos]) ctx
      //         if updatedContext.Success then
      //             let! r = run steps (pos + 1) updatedContext
      //             return r
      //         else
      //             return updatedContext
      // }
      //  return! run steps 0 execContext

      let lastStep = (Array.length steps) - 1
      let mutable ctx = execContext

      for i in [ 0..lastStep ] do
        if lastStep = i then
          let sum = ctx.Steps |> List.sumBy (fun (_, ts) -> ts.TotalMilliseconds)
          let ok = Res.isOk ctx.Response

          if not ok then
            ctx.Io.Value.Log.Error $"{Res.getError ctx.Response}"

          ctx <-
            { ctx with
                Elapsed = Some(TimeSpan.FromMilliseconds sum) }
        //Success = Res.isOk ctx.Response  }
        else
          let! updatedContext = execStep (steps[i]) ctx
          ctx <- updatedContext

      return ctx
    }
