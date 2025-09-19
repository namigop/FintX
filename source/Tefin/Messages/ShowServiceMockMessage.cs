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
    public string Dll { get; } = dll;
    public string ServiceName { get; } = serviceName;
    public string[] CsFiles { get; } = csFiles;
    public uint Port { get; } = port;
    public string Description { get; } = description;
    public CompileOutput Output { get; } = output;
    public string ProtoFileOrUrl { get; } = protoFileOrUrl;
    public string? SelectedDiscoveredService { get; } = selectedDiscoveredService;

    public required bool Reset { get; init; } = false;
}