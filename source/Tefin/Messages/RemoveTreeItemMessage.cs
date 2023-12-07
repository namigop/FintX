using Tefin.ViewModels.Explorer;

namespace Tefin.Messages;

public class RemoveTreeItemMessage : MessageBase {
    public RemoveTreeItemMessage(IExplorerItem item) {
        this.Item = item;
    }
    public IExplorerItem Item { get; }
}