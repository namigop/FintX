using Tefin.ViewModels.Explorer;

namespace Tefin.Messages;

public class RemoveTreeItemMessage(IExplorerItem item) : MessageBase {
    public IExplorerItem Item { get; } = item;
}