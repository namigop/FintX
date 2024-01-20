namespace Tefin.ViewModels.Tabs;

public interface IListEditorViewModel {
    public Type ListType { get; }

    void AddItem(object item);

    void Clear();

    public (bool, object) GetList();

    public IEnumerable<object> GetListItems();

    public void Show(object listInstance);
}