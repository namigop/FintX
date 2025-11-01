using System.Collections.ObjectModel;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Utils;

namespace Tefin.ViewModels.Tabs;

public class EnvDataViewModel : ViewModelBase {
    private string _description = "";
    private string _name = "";


    public EnvDataViewModel(EnvConfigData envData) {
        this.AddNewRowCommand = ReactiveCommand.Create(this.OnAddNewRow);
        this.RemoveRowCommand = ReactiveCommand.Create<EnvVarViewModel>(this.OnRemoveRow);

        this.Name = envData.Name;
        this.Description = envData.Description;
        this.Variables = envData.Variables
            .Select(v => new EnvVarViewModel(v, this.RemoveRowCommand))
            .Then(v => new ObservableCollection<EnvVarViewModel>(v));
    }

    public ICommand AddNewRowCommand { get; }

    public string Description {
        get => this._description;
        set => this.RaiseAndSetIfChanged(ref this._description, value);
    }

    public string Name {
        get => this._name;
        set => this.RaiseAndSetIfChanged(ref this._name, value);
    }

    public ICommand RemoveRowCommand { get; }

    public ObservableCollection<EnvVarViewModel> Variables { get; }

    public string GenerateFileContent() {
        var variables = this.Variables.Select(v => v.ToEnvVar());
        var cfg = EnvConfig.createConfig(this.Name, this.Description);
        cfg.Variables.AddRange(variables);
        return Instance.jsonSerialize(cfg);
    }

    private void OnAddNewRow() {
        var envVar = new EnvVar("{{REPLACE_THIS}}", "_current_", "_default_", "", "string");
        var vm = new EnvVarViewModel(envVar, this.RemoveRowCommand);
        this.Variables.Add(vm);
    }

    private void OnRemoveRow(EnvVarViewModel arg) => this.Variables.Remove(arg);
}