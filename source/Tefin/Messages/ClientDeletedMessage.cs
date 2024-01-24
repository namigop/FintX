#region

using Tefin.Core.Interop;

#endregion

namespace Tefin.Messages;

public class ClientDeletedMessage(ProjectTypes.ClientGroup client) : MessageBase {
    public ProjectTypes.ClientGroup Client { get; } = client;
}