#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.Messages;

public class RemoveTreeItemMessage(IExplorerItem item) : MessageBase {
    public IExplorerItem Item { get; } = item;
}