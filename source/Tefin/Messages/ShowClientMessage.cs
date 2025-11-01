#region

using Tefin.Core.Build;

#endregion

namespace Tefin.Messages;

public class ShowClientMessage(
    CompileOutput output,
    string protoFilesOrUrl,
    string clientName,
    string? selectedDiscoveredService,
    string description,
    string[] csFiles,
    string dll)
    : MessageBase {
    public string ClientName { get; } = clientName;
    public string[] CsFiles { get; } = csFiles;
    public string Description { get; } = description;
    public string Dll { get; } = dll;
    public CompileOutput Output { get; } = output;
    public string ProtoFilesOrUrl { get; } = protoFilesOrUrl;

    public required bool Reset { get; init; } = false;
    public string? SelectedDiscoveredService { get; } = selectedDiscoveredService;
}