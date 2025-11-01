using System.Collections.ObjectModel;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;

namespace Tefin.ViewModels.ProjectEnv;

public class EnvMenuViewModel : ViewModelBase {
    private GroupEnvConfigData _envVariables = null!;
    private EnvSelection? _selectedEnvironment;

    public ObservableCollection<EnvSelection> Environments { get; } = [];

    public EnvSelection? SelectedEnvironment {
        get => this._selectedEnvironment;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedEnvironment, value);
            if (this._selectedEnvironment != null) {
                foreach (var e in this.Environments) {
                    e.IsSelected = false;
                }

                this._selectedEnvironment.IsSelected = true;
                GlobalHub.publish(
                    new EnvSelectedMessage(this._selectedEnvironment.Name, this._selectedEnvironment.Path));
            }
        }
    }

    public void Init(string projectPath) {
        this.Environments.Clear();
        this._envVariables = VarsStructure.getVarsForProject(this.Io, projectPath);
        foreach (var (file, env) in this._envVariables.Variables) {
            this.Environments.Add(new EnvSelection(file, env));
        }

        if (this.Environments.Count == 0) {
            var envConfig = EnvConfig.createConfig("Default", "Default environment");
            VarsStructure.saveEnvForProject(this.Io, envConfig, projectPath);
            this.Init(projectPath);
        }

        this.SelectedEnvironment = this.Environments.FirstOrDefault()!;
    }
}