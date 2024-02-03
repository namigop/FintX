#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingReqViewModel(
    MethodInfo methodInfo,
    bool generateFullTree,
    List<object?>? methodParameterInstances = null)
    : UnaryReqViewModel(methodInfo, generateFullTree, methodParameterInstances);