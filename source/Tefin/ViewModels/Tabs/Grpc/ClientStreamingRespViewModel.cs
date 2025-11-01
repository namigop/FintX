#region

using System.Reflection;

using Tefin.Core.Execution;
using Tefin.Core.Interop;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingRespViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg)
    : StandardResponseViewModel(methodInfo, cg) {
    public override void Init(AllVariableDefinitions envVariables) {
        this.ResponseVariables = envVariables.ResponseVariables;
        this.ResponseEditor.Init();
    }

    public override void Show(bool ok, object response, Context context) {
        //base.Show(ok, response, context);
    }
}