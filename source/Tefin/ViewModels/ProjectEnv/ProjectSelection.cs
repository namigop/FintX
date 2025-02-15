using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI;

using File = System.IO.File;

namespace Tefin.ViewModels.ProjectEnv;

public class ProjectSelection(string package, string path) : ViewModelBase {
    private bool _isSelected;
    public string Name => System.IO.Path.GetFileName(path);
    public string Path => path;
    public string Package => package;

    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }
}