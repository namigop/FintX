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
    string[] csFiles)
    : MessageBase {
    public string ClientName { get; } = clientName;
    public string[] CsFiles { get; } = csFiles;
    public string Description { get; } = description;
    public CompileOutput Output { get; } = output;
    public string ProtoFilesOrUrl { get; } = protoFilesOrUrl;
    public string? SelectedDiscoveredService { get; } = selectedDiscoveredService;
}