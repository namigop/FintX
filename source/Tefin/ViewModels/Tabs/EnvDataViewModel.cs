using System.Collections.ObjectModel;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Utils;

namespace Tefin.ViewModels.Tabs;

public class EnvDataViewModel : ViewModelBase {
    private readonly ObservableCollection<EnvVarViewModel> _variables;
    private string _description = "";
    private string _name = "";

   
    public EnvDataViewModel(EnvConfigData envData) {
        this.AddNewRowCommand = ReactiveCommand.Create(OnAddNewRow);
        this.RemoveRowCommand = ReactiveCommand.Create<EnvVarViewModel>(OnRemoveRow);

        this.Name = envData.Name;
        this.Description = envData.Description;
        this._variables = envData.Variables
            .Select(v => new EnvVarViewModel(v, this.RemoveRowCommand))
            .Then(v => new ObservableCollection<EnvVarViewModel>(v));
      
    }

    private void OnRemoveRow(EnvVarViewModel arg) {
        this.Variables.Remove(arg);
    }

    public ICommand AddNewRowCommand { get; }
    public ICommand RemoveRowCommand { get; }
    private void OnAddNewRow() {
        var envVar = new EnvVar("{{REPLACE_THIS}}", "_current_", "_default_", "", "string");
        var vm = new EnvVarViewModel(envVar, this.RemoveRowCommand);
        this.Variables.Add(vm);
    }

    public ObservableCollection<EnvVarViewModel> Variables => this._variables;

    public string Description {
        get => this._description;
        set => this.RaiseAndSetIfChanged(ref this._description , value);
    }

    public string Name {
        get => this._name;
        set => this.RaiseAndSetIfChanged(ref this._name , value);
    }

    public string GenerateFileContent() {
        var variables = this.Variables.Select(v => v.ToEnvVar());
        var cfg = EnvConfig.createConfig(this.Name, this.Description);
        cfg.Variables.AddRange(variables);
        return Instance.jsonSerialize(cfg);
    }
}