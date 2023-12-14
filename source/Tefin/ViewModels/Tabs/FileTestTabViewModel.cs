#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FileTestTabViewModel : TabViewModelBase {
    public FileTestTabViewModel(FileTestNode item) : base(item) {
    }

    protected override string GetTabId() {
        return ((FileNode)this.ExplorerItem).FullPath;
    }
}