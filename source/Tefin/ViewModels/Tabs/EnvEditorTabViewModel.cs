using System.Windows.Input;

using ReactiveUI;

using Tefin.ViewModels.Explorer.Config;

namespace Tefin.ViewModels.Tabs;

public class EnvEditorTabViewModel : TabViewModelBase {
    private const string _icon = "";
    private EnvDataViewModel _envData;

    public EnvEditorTabViewModel(EnvNode item) : base(item) {
        this._envData = new EnvDataViewModel(item.GetEnvData());
    }

    public EnvDataViewModel EnvData {
        get => this._envData;
        private set => this.RaiseAndSetIfChanged(ref  _envData , value);
    }

    public override string Icon => _icon;
 
    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
        
    }

    protected override string GetTabId() => ((EnvNode)this.ExplorerItem).Title;
}