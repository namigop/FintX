#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FileTestTabViewModel : TabViewModelBase {
    public FileTestTabViewModel(FileTestNode item) : base(item) {
    }
    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }
    protected override string GetTabId() {
        return ((FileNode)this.ExplorerItem).FullPath;
    }
    public override void Import(string reqFile) {
        throw new NotImplementedException();
    }
}