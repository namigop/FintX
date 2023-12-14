#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Tabs;

public class ListJsonEditorViewModel : ViewModelBase, IListEditorViewModel {
    private readonly MethodInfo _addMethod;
    private readonly Type _listItemType;
    private readonly string _name;
    private string _json;
    private object? _listInstance;

    public ListJsonEditorViewModel(string name, Type listType) {
        this._name = name;
        this.ListType = listType;
        this._listInstance = Activator.CreateInstance(listType);
        this._listItemType = TypeHelper.getListItemType(listType).Value;
        this._addMethod = listType.GetMethod("Add")!;
        this._json = "";
    }

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public Type ListType {
        get;
    }

    public (bool, object) GetList() {
        try {
            var list = Instance.indirectDeserialize(this.ListType, this._json);
            return (true, list);
        }
        catch {
            return (false, default!);
        }
    }

    public void Clear() {
        this.Json = "";
    }
    public void AddItem(object instance) {
        this._addMethod.Invoke(this._listInstance, new[] {
            instance
        });
        var jsonInstance = Instance.indirectSerialize(this._listItemType, instance);
        var startIndex = this.Json.Length - 1;
        var endPos = this.Json.LastIndexOf(']', startIndex);
        var begin = endPos == 1 ? "" : ",";
        this.Json = this._json.Insert(endPos, $"{begin}{Environment.NewLine}{jsonInstance}{Environment.NewLine}");
    }

    public IEnumerable<object> GetListItems() {
        dynamic list = Instance.indirectDeserialize(this.ListType, this._json);
        foreach (var i in list)
            yield return i;
    }
    public void Show(object listInstance) {
        try {
            this._listInstance = listInstance;
            var json = Instance.indirectSerialize(this.ListType, listInstance);
            this.Json = json;
        }
        catch (Exception exc) {
            this.Io.Log.Error($"Unable to create an instance for {this._listItemType}");
            this.Io.Log.Error(exc);
        }
    }
}