using System.Reflection;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingReqViewModel : UnaryReqViewModel {

    public ServerStreamingReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object?>? methodParameterInstances = null) 
        : base(methodInfo, generateFullTree, methodParameterInstances) {
    }
}