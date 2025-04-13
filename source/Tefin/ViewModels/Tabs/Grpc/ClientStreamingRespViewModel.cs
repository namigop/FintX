#region

using System.Reflection;

using Tefin.Core.Execution;
using Tefin.Core.Interop;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingRespViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) : StandardResponseViewModel(methodInfo, cg) {
    public override void Show(bool ok, object response, Context context) {
        //base.Show(ok, response, context);
    }
}