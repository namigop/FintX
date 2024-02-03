#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FilePerfTabViewModel(FilePerfNode item) : TabViewModelBase(item) {
    public override string Icon { get; } = "";

    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }

    protected override string GetTabId() => ((FileNode)this.ExplorerItem).FullPath;

    protected override Task OnClose() => base.OnClose();
}