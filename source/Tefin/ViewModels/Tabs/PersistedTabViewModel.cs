using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Tabs;

public abstract class PersistedTabViewModel : TabViewModelBase {
    protected PersistedTabViewModel(IExplorerItem item) : base(item) {
    }

    public override bool CanAutoSave => true;
    public abstract ProjectTypes.ClientGroup Client { get; }

    public abstract ClientMethodViewModelBase ClientMethod { get; }

    public abstract string GetRequestContent();

    public abstract override void Init();

    public abstract void UpdateTitle(string oldFullPath, string newFullPath);
}