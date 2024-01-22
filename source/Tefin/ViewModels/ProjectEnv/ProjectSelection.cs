namespace Tefin.ViewModels.ProjectEnv;

public class ProjectSelection (string path) : ViewModelBase {
    public string Name { get => System.IO.Path.GetFileName(path); }
    public string Path { get => path; }
}