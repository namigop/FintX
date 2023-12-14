#region

using Tefin.Core.Build;

#endregion

namespace Tefin.Messages;

public class ShowClientMessage : MessageBase {
    public ShowClientMessage(CompileOutput output, string protoFilesOrUrl, string clientName, string? selectedDiscoveredService, string description, string[] csFiles) {
        this.Output = output;
        this.ProtoFilesOrUrl = protoFilesOrUrl;
        this.ClientName = clientName;
        this.SelectedDiscoveredService = selectedDiscoveredService;
        this.Description = description;
        this.CsFiles = csFiles;
    }

    public string ClientName { get; }
    public string[] CsFiles { get; }
    public string Description { get; }
    public CompileOutput Output { get; }
    public string ProtoFilesOrUrl { get; }
    public string? SelectedDiscoveredService { get; }
}