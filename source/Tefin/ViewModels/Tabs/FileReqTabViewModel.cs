#region

using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class FileReqTabViewModel : PersistedTabViewModel {
    public FileReqTabViewModel(FileReqNode item) : base(item) {
        this.ClientMethod = item.CreateViewModel();
        this.Client = ((MethodNode)item.Parent).Client;
        this.ClientMethod.SubscribeTo(x => x.IsBusy, this.OnIsBusyChanged);
    }
    public override ClientMethodViewModelBase ClientMethod { get; }
    public override ProjectTypes.ClientGroup Client { get; }
    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.FilePath);
        this.ClientMethod.ImportRequestFile(this.FilePath);
    }
    
    public string FilePath {
        get => ((FileNode)this.ExplorerItem).FullPath;
    }
    protected override string GetTabId() {
        return this.FilePath;
    }
    public override void Dispose() {
        base.Dispose();
        this.ClientMethod.Dispose();
    }

    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
    }
    public override string GetRequestContent() {
        return this.ClientMethod.GetRequestContent();
    }

    public override void UpdateTitle(string oldFullPath, string newFullPath) {
        //Note: the corresponding node has already been updated
        this.Title = Path.GetFileName(this.FilePath);
        this.Id = Path.GetFileName(this.FilePath);
    }
}