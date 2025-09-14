using Tefin.Core.Build;

namespace Tefin.Messages;

public class ShowServiceMockMessage(
    CompileOutput output,
    string protoFilesOrUrl,
    string clientName,
    string? selectedDiscoveredService,
    string description,
    string[] csFiles, string dll)
    : MessageBase {
    public string Dll { get; } = dll;
    public string ClientName { get; } = clientName;
    public string[] CsFiles { get; } = csFiles;
    public string Description { get; } = description;
    public CompileOutput Output { get; } = output;
    public string ProtoFilesOrUrl { get; } = protoFilesOrUrl;
    public string? SelectedDiscoveredService { get; } = selectedDiscoveredService;

    public required bool Reset { get; init; } = false;
}