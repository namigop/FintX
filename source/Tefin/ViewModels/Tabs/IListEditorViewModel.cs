namespace Tefin.ViewModels.Tabs;

public interface IListEditorViewModel {
    public Type ListType { get; }

    public (bool, object) GetList();

    public IEnumerable<object> GetListItems();

    public void Show(object listInstance);

    void Clear();

    void AddItem(object item);
}