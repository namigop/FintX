using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs;

public interface IListEditorViewModel {
    public Type ListType { get; }

    void AddItem(object item);

    void Clear();

    (bool, object) GetList();

    IEnumerable<object> GetListItems();

    void RemoveSelectedItem();

    void Show(object listInstance, List<VarDefinition> envVars);
}