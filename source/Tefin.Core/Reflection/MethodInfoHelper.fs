namespace Tefin.Core.Reflection

open System.Reflection
open System.Threading

module MethodInfoHelper =
    let getMethodInfoParameterInstances (methodInfo: MethodInfo) : obj[] =
        let methodArguments =
            methodInfo.GetParameters()
            |> Array.map (fun p ->
                //By default we will always pass a cancellation token. This will give th user a chance to cancel streams.
                if (p.IsOptional && p.ParameterType = typeof<CancellationToken>) then
                    let struct (_, inst) = TypeBuilder.getDefault p.ParameterType true None 0
                    inst
                else if (p.IsOptional) then
                    TypeHelper.getDefault p.ParameterType
                else
                    let struct (_, inst) = TypeBuilder.getDefault p.ParameterType true None 0
                    inst)

        methodArguments
