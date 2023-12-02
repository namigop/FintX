#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FileReqTabViewModel : TabViewModelBase {

    public FileReqTabViewModel(FileReqNode item) : base(item) {
    }

    protected override string GetTabId() {
        return ((FileNode)this.ExplorerItem).FullPath;
    }
}