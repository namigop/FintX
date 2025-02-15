using ReactiveUI;

using Tefin.Core;

namespace Tefin.ViewModels.ProjectEnv;

public class EnvSelection(string file, EnvConfigData data) : ViewModelBase {
    private bool _isSelected;
    public string Name => data.Name;
    public string Path => file;
    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }
}