using System.Linq;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;

namespace Tefin.ViewModels.Tabs;

public class ClientStreamJsonEditorViewModel : ViewModelBase, IListEditorViewModel {
    private readonly object? _listInstance;
    private readonly Type _listItemType;
    private string _json;

    public ClientStreamJsonEditorViewModel(Type listType) {
        this.ListType = listType;
        this._listInstance = Activator.CreateInstance(listType);
        this._listItemType = TypeHelper.getListItemType(listType).Value;
        this._json = "";
    }

    public Type ListType {
        get;
    }

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public (bool, object) GetList() {
        try {
            var list = Instance.indirectDeserialize(this.ListType, this._json);
            return (true, list);
        }
        catch (Exception e) {
            return (false, default!);
        }
    }
    public IEnumerable<object> GetListItems() {
        dynamic list = Instance.indirectDeserialize(this.ListType, this._json);
        foreach (var i in list)
            yield return i;
        // }
        // catch (Exception e) {
        //     return Enumerable.Empty<object>();
        // }
    }
    public void Show(object listInstance) {
        try {
            var json = Instance.indirectSerialize(this.ListType, listInstance);
            this.Json = json;
        }
        catch (Exception exc) {
            this.Io.Log.Error($"Unable to create an instance for {this._listItemType}");
            this.Io.Log.Error(exc);
        }
    }
}