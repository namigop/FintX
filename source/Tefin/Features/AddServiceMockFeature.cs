using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc;

namespace Tefin.Features;

public class AddServiceMockFeature(
    ProjectTypes.Project project,
    string serviceName,
    string protoOrUrl,
    string description,
    string[] csFiles,
    string dll,
    uint port,
    MethodInfo[] methods,
    IOs io) {
    public void Add() {
        ServiceMockStructure.addServiceMock(
            io,
            project,
            serviceName,
            description,
            csFiles,
            dll, 
            port,
            methods);
    }
}