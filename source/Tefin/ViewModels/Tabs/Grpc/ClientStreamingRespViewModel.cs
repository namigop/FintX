#region

using System.Reflection;

using Tefin.Core.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingRespViewModel(MethodInfo methodInfo) : StandardResponseViewModel(methodInfo) {
    public override void Show(bool ok, object response, Context context) {
        //base.Show(ok, response, context);
    }
}