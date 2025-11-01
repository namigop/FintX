using Tefin.Core.Build;

namespace Tefin.Messages;

public class ShowServiceMockMessage(
    CompileOutput output,
    string protoFileOrUrl,
    string serviceName,
    string? selectedDiscoveredService,
    string description,
    string[] csFiles,
    uint port,
    string dll)
    : MessageBase {
    public string[] CsFiles { get; } = csFiles;
    public string Description { get; } = description;
    public string Dll { get; } = dll;
    public CompileOutput Output { get; } = output;
    public uint Port { get; } = port;
    public string ProtoFileOrUrl { get; } = protoFileOrUrl;

    public required bool Reset { get; init; } = false;
    public string? SelectedDiscoveredService { get; } = selectedDiscoveredService;
    public string ServiceName { get; } = serviceName;
}