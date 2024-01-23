using ReactiveUI;

namespace Tefin.ViewModels.ProjectEnv;

public class ProjectSelection (string path) : ViewModelBase {
    private bool _isSelected;
    public string Name { get => System.IO.Path.GetFileName(path); }
    public string Path { get => path; }

    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}