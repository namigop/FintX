namespace Tefin.ViewModels.Tabs;

public interface IListEditorViewModel {
    public Type ListType { get; }

    void AddItem(object item);

    void Clear();

    (bool, object) GetList();

    IEnumerable<object> GetListItems();

    void Show(object listInstance);

    void RemoveSelectedItem();
}