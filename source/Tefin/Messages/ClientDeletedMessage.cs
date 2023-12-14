#region

using Tefin.Core.Interop;

#endregion

namespace Tefin.Messages;

public class ClientDeletedMessage : MessageBase {
    public ClientDeletedMessage(ProjectTypes.ClientGroup client) {
        this.Client = client;
    }
    public ProjectTypes.ClientGroup Client { get; }
}