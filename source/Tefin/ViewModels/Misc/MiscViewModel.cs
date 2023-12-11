using System.Collections.ObjectModel;

using ReactiveUI;

namespace Tefin.ViewModels.Misc;

public class MiscViewModel : ViewModelBase {
    private MiscViewModelTabItem _selectedItem;

    public MiscViewModel() {
        this.MiscItems.Add(new OutputMiscViewModel());
        this.MiscItems.Add(new ChartMiscViewModel());
        this._selectedItem = this.MiscItems[0];
    }

    public MiscViewModelTabItem SelectedItem {
        get => this._selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<MiscViewModelTabItem> MiscItems { get; } = new();
}