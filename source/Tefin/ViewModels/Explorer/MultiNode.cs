namespace Tefin.ViewModels.Explorer;

public class MultiNode(IExplorerItem[] items) : NodeBase {
    public IExplorerItem[] Items { get; } = items;
    public override void Init(){}
}