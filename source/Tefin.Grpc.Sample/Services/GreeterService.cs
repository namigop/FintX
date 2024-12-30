using Grpc.Core;
using Tefin.Grpc.Sample;

namespace Tefin.Grpc.Sample.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override Task<ByeReply> SayBye(ByeRequest request, ServerCallContext context) {
        return Task.FromResult(new ByeReply
        {
            Message = "Bye " + request.Name
        });
    }

    public override Task<EnumResponse> SayEnum(EnumRequest request, ServerCallContext context) {
        var msg = new EnumMsg();
        msg.SomeEnums.Add(SampleEnum.A);
        msg.SomeEnums.Add(SampleEnum.B);
        msg.SomeEnums.Add(SampleEnum.A);
        
        var resp = new EnumResponse() { Response = msg };
        return Task.FromResult(resp);
    }

    public override async Task Duplex(IAsyncStreamReader<ByeRequest> requestStream,
        IServerStreamWriter<EnumResponse> responseStream, ServerCallContext context) {
        while (await requestStream.MoveNext()) {
            var foo = requestStream.Current;
            await responseStream.WriteAsync(new EnumResponse() {
                Response =  new EnumMsg()
            });
        }
    }
}