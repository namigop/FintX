using Tefin.ViewModels.Explorer.Config;

namespace Tefin.ViewModels.Tabs;

public class EnvEditorTabViewModel(EnvNode item) : TabViewModelBase(item) {
    public override string Icon { get; } = "";

    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }

    protected override string GetTabId() => ((EnvNode)this.ExplorerItem).Title;
}