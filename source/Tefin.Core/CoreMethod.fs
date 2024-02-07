namespace Tefin.Core

open System.Reflection

type MethodType =
  | Unary = 0
  | ServerStreaming = 1
  | ClientStreaming = 2
  | Duplex = 3

module CoreMethod =
  let paramsToPropInfos (methodInfo: MethodInfo) (requestParams: obj array) =
    methodInfo.GetParameters()
    |> Array.map (fun p -> p.Name)
    |> Array.zip requestParams
    |> Array.map (fun (i, name) -> { PropInfoName = name; Value = i })
