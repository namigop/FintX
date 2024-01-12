#region

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public class MethodTabViewModel : PersistedTabViewModel {
    private readonly string? _requestFile;
    public MethodTabViewModel(MethodNode item, string requestFile = "") : base(item) {
        this._requestFile = requestFile;
        this.ClientMethod = item.CreateViewModel();
        this.Client = item.Client;
        this.ClientMethod.SubscribeTo(x => x.IsBusy, this.OnIsBusyChanged);
    }

    public override string Icon { get; } = "";//"Icon.Method";
    public override ProjectTypes.ClientGroup Client { get; }

    public override ClientMethodViewModelBase ClientMethod { get; }

    public override void Dispose() {
        base.Dispose();
        this.ClientMethod.Dispose();
    }

    public override string GetRequestContent() {
        return this.ClientMethod.GetRequestContent();
    }

    public override void Init() {
        if (!string.IsNullOrEmpty(this._requestFile)) {
            this.Id = this._requestFile;
            this.ClientMethod.ImportRequestFile(this._requestFile);
        }
        else {
            this.Id = this.GetTabId();
        }

        this.Title = Path.GetFileNameWithoutExtension(this.Id);
    }

    protected override string GetTabId() {
        var id = AutoSave.getAutoSaveLocation(this.Io, this.ClientMethod.MethodInfo, this.Client.Path);
        return id;
    }

    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
    }

    public override void UpdateTitle(string oldFullPath, string newFullPath) {
        this.Id = newFullPath;
        this.Title = Path.GetFileNameWithoutExtension(this.Id);
    }
}