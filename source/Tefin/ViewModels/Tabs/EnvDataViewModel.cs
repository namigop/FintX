using System.Collections.ObjectModel;

using ReactiveUI;

using Tefin.Core;
using Tefin.Utils;

namespace Tefin.ViewModels.Tabs;

public class EnvDataViewModel : ViewModelBase {
    private readonly ObservableCollection<EnvVarViewModel> _variables;
    private string _description = "";
    private string _name = "";

    public EnvDataViewModel(EnvConfigData envData) {
        this.Name = envData.Name;
        this.Description = envData.Description;
        this._variables = envData.Variables
            .Select(v => new EnvVarViewModel(v))
            .Then(v => new ObservableCollection<EnvVarViewModel>(v));
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
}