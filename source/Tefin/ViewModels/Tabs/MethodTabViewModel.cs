#region

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class MethodTabViewModel : TabViewModelBase {
    public MethodTabViewModel(MethodNode item) : base(item) {
        this.ClientMethod = item.CreateViewModel();
        this.Client = item.Client;

        this.ClientMethod.SubscribeTo(x => x.IsBusy, this.OnIsBusyChanged);
        this.AllowDuplicates = true;
    }

    public ProjectTypes.ClientGroup Client {
        get;
    }

    public ClientMethodViewModelBase ClientMethod { get; }

    public override void Dispose() {
        base.Dispose();
        this.ClientMethod.Dispose();
    }

    public string GetRequestContent() {
        return this.ClientMethod.GetRequestContent();
    }

    public override void Import(string reqFile) {
        this.ClientMethod.ImportRequest(reqFile);
    }
    
    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileNameWithoutExtension(this.Id);
    }

    protected override string GetTabId() {
        var id = AutoSave.getSaveLocation(this.Io, this.ClientMethod.MethodInfo, this.Client.Path);
        return id;
    }

    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
    }
}