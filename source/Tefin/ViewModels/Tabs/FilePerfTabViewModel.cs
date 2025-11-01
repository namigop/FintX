#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FilePerfTabViewModel(FilePerfNode item) : TabViewModelBase(item) {
    public override string Icon { get; } = "";

    protected override string GetTabId() => ((FileNode)this.ExplorerItem).FullPath;

    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }

    protected override Task OnClose() => base.OnClose();
}