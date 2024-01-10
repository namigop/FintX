#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FileReqTabViewModel(FileReqNode item) : TabViewModelBase(item) {
    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }
    protected override string GetTabId() {
        return ((FileNode)this.ExplorerItem).FullPath;
    }  
     
}