#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingReqViewModel : UnaryReqViewModel {
    public ServerStreamingReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object?>? methodParameterInstances = null)
        : base(methodInfo, generateFullTree, methodParameterInstances) {
    }
}