using System.Reflection;

using Tefin.Core;

using static Tefin.Core.Interop.ProjectTypes;

namespace Tefin.Features;

public class AddServiceMockFeature(
    Project project,
    string serviceName,
    string protoOrUrl,
    string description,
    string[] csFiles,
    string dll,
    uint port,
    bool isUsingNamedPipes,
    NamedPipeServerConfig? namedPipeServerConfig,
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
            GetServerConfig(),
            methods);
        return;

        ServerTransportConfig GetServerConfig() {
            if (isUsingNamedPipes)
                return ServerTransportConfig.NewNamedPipeServer(namedPipeServerConfig);
            return  ServerTransportConfig.DefaultServer;
        }
    }
}