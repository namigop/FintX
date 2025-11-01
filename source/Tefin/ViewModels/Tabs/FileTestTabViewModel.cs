#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FileTestTabViewModel(FileTestNode item) : TabViewModelBase(item) {
    public override string Icon { get; } = "";

    protected override string GetTabId() => ((FileNode)this.ExplorerItem).FullPath;

    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }
}