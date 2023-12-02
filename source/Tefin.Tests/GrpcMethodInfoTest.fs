module GrpcMethodInfoTest

open System
open System.Threading
open System.Threading.Tasks
open Google.Protobuf.Collections
open Grpc.Core
open Tefin.Grpc
open Xunit
open Tefin.Core.Reflection

type Request() =
    member val IntType = 0 with get, set
    member val Int16Type = 0s with get, set
    member val Int64Type = 0L with get, set
    member val DoubleType = 0.0 with get, set
    member val DecimalType = 0.0m with get, set
    member val SingleType = 0f with get, set
    member val UIntType = 0u with get, set
    member val UInt16Type = 0us with get, set
    member val UInt64Type = 0uL with get, set
    member val BoolType = false with get, set
    member val DateTimeType = Unchecked.defaultof<DateTime> with get, set
    member val DateTimeOffsetType = Unchecked.defaultof<DateTimeOffset> with get, set
    member val GuidType = Unchecked.defaultof<Guid> with get, set
    member val TimeSpanType = Unchecked.defaultof<TimeSpan>  with get, set
    member val CancellationTokenType =  Unchecked.defaultof<CancellationToken> with get, set
    member val CharType =  Unchecked.defaultof<char> with get, set
    member val ByteType =  10uy with get, set   
    member val StringType = "abc" with get, set
    member val RepeatedType = RepeatedField<string>() with get, set
    
type Response() =
  member val Result = true with get,set
type TestGrpcClient() =
    member x.Method(req:Request, headers:Metadata, deadline:Nullable<DateTime>, token:CancellationToken) =
       Response()
    member x.MethodWithOptions(req:Request, options:CallOptions) =
       Response()
       
[<Fact>]
let ``Can inspect methodInfo``() =
    let mi= typeof<TestGrpcClient>.GetMethod("Method")
    TypeBuilder.register GrpcTypeBuilder.getDefault
    let objArray = MethodInfoHelper.getMethodInfoParameterInstances mi
    Assert.Equal(4, objArray.Length)
    Assert.NotNull(objArray.[0])
    Assert.NotNull(objArray.[1])
    Assert.NotNull(objArray.[2])
    Assert.NotNull(objArray.[3])
    
    Assert.Equal(typeof<Request>, objArray.[0].GetType())
    Assert.Equal(typeof<Metadata>, objArray.[1].GetType())
    Assert.Equal(typeof<DateTime>, objArray.[2].GetType())
    Assert.Equal(typeof<CancellationToken>, objArray.[3].GetType())

[<Fact>]
let ``Can inspect methodInfo with CallOptions param``() =
    let mi= typeof<TestGrpcClient>.GetMethod("MethodWithOptions")
    TypeBuilder.register GrpcTypeBuilder.getDefault
    let objArray = MethodInfoHelper.getMethodInfoParameterInstances mi
    Assert.Equal(2, objArray.Length)
    Assert.NotNull(objArray.[0])
    Assert.NotNull(objArray.[1])
    
    Assert.Equal(typeof<Request>, objArray.[0].GetType())
    Assert.Equal(typeof<CallOptions>, objArray.[1].GetType())
    
    
(*{
      /// <summary>Creates a new client for Greeter</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public GreeterClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for Greeter that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public GreeterClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected GreeterClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected GreeterClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloReply SayHello(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHello(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloReply SayHello(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SayHello, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloReply> SayHelloAsync(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloReply> SayHelloAsync(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SayHello, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloReply RandomError(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RandomError(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloReply RandomError(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_RandomError, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloReply> RandomErrorAsync(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RandomErrorAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloReply> RandomErrorAsync(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_RandomError, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::Greet.HelloReply> ServerStream(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ServerStream(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncServerStreamingCall<global::Greet.HelloReply> ServerStream(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_ServerStream, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncClientStreamingCall<global::Greet.HelloRequest, global::Greet.HelloReply> ClientStream(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ClientStream(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncClientStreamingCall<global::Greet.HelloRequest, global::Greet.HelloReply> ClientStream(grpc::CallOptions options)
      {
        return CallInvoker.AsyncClientStreamingCall(__Method_ClientStream, null, options);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::Greet.HelloRequest, global::Greet.HelloReply> DuplexStream(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return DuplexStream(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncDuplexStreamingCall<global::Greet.HelloRequest, global::Greet.HelloReply> DuplexStream(grpc::CallOptions options)
      {
        return CallInvoker.AsyncDuplexStreamingCall(__Method_DuplexStream, null, options);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloByteResponse HelloBytes(global::Greet.HelloByteRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return HelloBytes(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloByteResponse HelloBytes(global::Greet.HelloByteRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_HelloBytes, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloByteResponse> HelloBytesAsync(global::Greet.HelloByteRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return HelloBytesAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloByteResponse> HelloBytesAsync(global::Greet.HelloByteRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_HelloBytes, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloRequest Echo(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Echo(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloRequest Echo(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Echo, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloRequest> EchoAsync(global::Greet.HelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return EchoAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloRequest> EchoAsync(global::Greet.HelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Echo, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloRequest2 Echo2(global::Greet.HelloRequest2 request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Echo2(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.HelloRequest2 Echo2(global::Greet.HelloRequest2 request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Echo2, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloRequest2> Echo2Async(global::Greet.HelloRequest2 request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Echo2Async(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.HelloRequest2> Echo2Async(global::Greet.HelloRequest2 request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Echo2, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.MapRequest Map(global::Greet.MapRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Map(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Greet.MapRequest Map(global::Greet.MapRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Map, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.MapRequest> MapAsync(global::Greet.MapRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return MapAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Greet.MapRequest> MapAsync(global::Greet.MapRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Map, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Google.Protobuf.WellKnownTypes.Empty Void(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Void(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::Google.Protobuf.WellKnownTypes.Empty Void(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Void, null, options, request);
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Google.Protobuf.WellKnownTypes.Empty> VoidAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return VoidAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual grpc::AsyncUnaryCall<global::Google.Protobuf.WellKnownTypes.Empty> VoidAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Void, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      protected override GreeterClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new GreeterClient(configuration);
      }
    }
*)

