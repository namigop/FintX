using System.Collections.ObjectModel;

using ReactiveUI;

using Tefin.Core;

namespace Tefin.ViewModels.ProjectEnv;

public class EnvMenuViewModel : ViewModelBase {
    private ProjectEnvConfigData _envVariables = null!;
    private EnvSelection? _selectedEnvironment;

    public void Init(string projectPath) {
        this.Environments.Clear();
        this._envVariables = VarsStructure.getVars(this.Io, projectPath);
        foreach (var (file, env) in this._envVariables.Variables) {
            this.Environments.Add(new EnvSelection(file, env));
        }

        if (this.Environments.Count == 0) {
            var envConfig = EnvConfig.createConfig("Default", "Default environment");
            VarsStructure.saveEnv(this.Io, envConfig, projectPath);
            Init(projectPath);
        }

        this.SelectedEnvironment = this.Environments.FirstOrDefault()!;
    }

    public ObservableCollection<EnvSelection> Environments { get; } = [];

    public EnvSelection? SelectedEnvironment {
        get => this._selectedEnvironment;
        set {
            this.RaiseAndSetIfChanged(ref _selectedEnvironment, value);
            if (this._selectedEnvironment != null) {
                foreach (var e in Environments)
                    e.IsSelected = false;

                this._selectedEnvironment.IsSelected = true;
            }
        }
    }
}