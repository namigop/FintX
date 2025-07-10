#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public class ListJsonEditorViewModel(string name, Type listType, ProjectTypes.ClientGroup cg, bool isRequest) : ViewModelBase, IListEditorViewModel {
    private readonly MethodInfo _addMethod = listType.GetMethod("Add")!;
    private readonly Type _listItemType = TypeHelper.getListItemType(listType).Value;
    private readonly string _name = name;
    private readonly bool _isRequest = isRequest;
    private string _json = "";
    private object? _listInstance = Activator.CreateInstance(listType);

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public Type ListType {
        get;
    } = listType;

    public void AddItem(object instance) {
        this._addMethod.Invoke(this._listInstance, [instance]);
        var jsonInstance = Instance.indirectSerialize(this._listItemType, instance);
        var startIndex = this.Json.Length - 1;
        var endPos = this.Json.LastIndexOf(']', startIndex);
        var begin = endPos == 1 ? "" : ",";
        this.Json = this._json.Insert(endPos, $"{begin}{Environment.NewLine}{jsonInstance}{Environment.NewLine}");
    }

    public void Clear() => this.Json = "";

    public void RemoveSelectedItem() {
        Io.Log.Info("Removing items is not supported with the json editor");
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

    public IEnumerable<object> GetListItems() {
        dynamic list = Instance.indirectDeserialize(this.ListType, this._json);
        foreach (var i in list) {
            yield return i;
        }
    }

    public void Show(object listInstance, List<VarDefinition> variables) {
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