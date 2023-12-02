using System.Collections.ObjectModel;

namespace Tefin.ViewModels.Misc;

public class MiscViewModel : ViewModelBase {

    public MiscViewModel() {
        this.MiscItems.Add(new OutputMiscViewModel());
        this.MiscItems.Add(new ChartMiscViewModel());
    }

    public ObservableCollection<MiscViewModelTabItem> MiscItems { get; } = new();
}