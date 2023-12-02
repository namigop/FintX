namespace Tefin.Grpc

open Tefin.Core.Reflection

module Startup =

    let init() = task {
        TypeBuilder.register GrpcTypeBuilder.getDefault
    }