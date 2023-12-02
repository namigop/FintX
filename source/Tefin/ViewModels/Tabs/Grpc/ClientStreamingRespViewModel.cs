using System.Reflection;

using Tefin.Core.Execution;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingRespViewModel : StandardResponseViewModel {
    public ClientStreamingRespViewModel(MethodInfo methodInfo) : base(methodInfo) {
    }

    public override void Show(bool ok, object response, Context context) {
        base.Show(ok, response, context);
    }
}