#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FilePerfTabViewModel : TabViewModelBase {
    public FilePerfTabViewModel(FilePerfNode item) : base(item) {
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

    protected override Task OnClose() {
        return base.OnClose();
    }
}