#region

using System.Collections.ObjectModel;

using ReactiveUI;

#endregion

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
        set => this.RaiseAndSetIfChanged(ref this._selectedItem, value);
    }

    public ObservableCollection<MiscViewModelTabItem> MiscItems { get; } = new();
}