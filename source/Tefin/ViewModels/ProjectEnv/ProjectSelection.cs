using ReactiveUI;

namespace Tefin.ViewModels.ProjectEnv;

public class ProjectSelection(string package, string path) : ViewModelBase {
    private bool _isSelected;

    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }

    public string Name => System.IO.Path.GetFileName(path);
    public string Package => package;
    public string Path => path;
}