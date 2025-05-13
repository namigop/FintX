using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Tabs;

public abstract class PersistedTabViewModel : TabViewModelBase {
    protected PersistedTabViewModel(IExplorerItem item) : base(item) {
    }
    
    public override bool CanAutoSave => true;
    public abstract ProjectTypes.ClientGroup Client { get; }

    public abstract ClientMethodViewModelBase ClientMethod { get; }

    public abstract string GenerateFileContent();

    public abstract override void Init();

    public abstract void UpdateTitle(string oldFullPath, string newFullPath);

    public virtual AutoSave.FileParam GetFileParam() {
        var json = this.GenerateFileContent();
        var title = this.Title;
        var fileParam = AutoSave.FileParam.Empty()
            .WithJson(json)
            .WithFullPath(this.Id)
            .WithHeader(title);
        return fileParam;
    }
}