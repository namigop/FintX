#region

using System.Reflection;

using Tefin.Core.Interop;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingReqViewModel(
    MethodInfo methodInfo,
    ProjectTypes.ClientGroup cg,
    bool generateFullTree,
    List<object?>? methodParameterInstances = null)
    : UnaryReqViewModel(methodInfo, cg, generateFullTree, methodParameterInstances);