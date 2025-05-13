#region

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;

#endregion

namespace Tefin.ViewModels.Tabs;

public sealed class MethodTabViewModel : PersistedTabViewModel {
    private readonly string? _requestFile;
    private ProjectTypes.ClientGroup _client;

    public MethodTabViewModel(MethodNode item, string requestFile = "") : base(item) {
        this._requestFile = requestFile;
        this.ClientMethod = item.CreateViewModel();
        this._client = item.Client;
        this.ClientMethod.SubscribeTo(x => x.IsBusy, this.OnIsBusyChanged);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);
    }
    private void OnClientUpdated(MessageProject.MsgClientUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.Client.Path == obj.Path || this.Client.Path == obj.PreviousPath) {
            this._client = obj.Client;
            this.Io.Log.Debug($"Updated clientInstance for tab {this.GetTabId()}");
        }
    }

    public override ProjectTypes.ClientGroup Client => this._client;

    public override ClientMethodViewModelBase ClientMethod { get; }
    public override string Icon { get; } = "Icon.Method";

    public override void Dispose() {
        base.Dispose();
        this.ClientMethod.Dispose();
    }

    public override string GenerateFileContent() => this.ClientMethod.GetRequestContent();

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

    public override void UpdateTitle(string oldFullPath, string newFullPath) {
        this.Id = newFullPath;
        this.Title = Path.GetFileNameWithoutExtension(this.Id);
    }

    protected override string GetTabId() {
        var id = AutoSave.getAutoSaveLocation(this.Io, this.ClientMethod.MethodInfo, this.Client.Path);
        return id;
    }

    private void OnIsBusyChanged(ViewModelBase obj) => this.IsBusy = obj.IsBusy;
}