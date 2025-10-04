using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Explorer.ServiceMock;
using Tefin.ViewModels.Tabs.Grpc;

namespace Tefin.ViewModels.Tabs;

public sealed class MockMethodTabViewModel : PersistedTabViewModel {
    private readonly string? _scriptFile;
    private ProjectTypes.ServiceMockGroup _serviceMock;

    public MockMethodTabViewModel(MockMethodNode item, string scriptFile = "") : base(item) {
        this._scriptFile = scriptFile;
        this.MockMethod = item.CreateViewModel();
        this._serviceMock = item.ServiceMock;
        this.MockMethod.SubscribeTo(x => x.IsBusy, this.OnIsBusyChanged);
        GlobalHub.subscribe<MessageProject.MsgServiceMockUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);
    }

    public GrpcMockMethodHostViewModel MockMethod { get; private set; }

    private void OnClientUpdated(MessageProject.MsgServiceMockUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.Client.Path == obj.Path || this.Client.Path == obj.PreviousPath) {
            this._serviceMock = obj.Client;
            this.Io.Log.Debug($"Updated clientInstance for tab {this.GetTabId()}");
        }
    }

    public ProjectTypes.ServiceMockGroup ServiceMock => this._serviceMock;
    public override ProjectTypes.ClientGroup Client => ProjectTypes.ClientGroup.Empty();

    public override ClientMethodViewModelBase ClientMethod { get; } = null!;
    public override string Icon { get; } = "Icon.Method";

    public override void Dispose() {
        base.Dispose();
    }

    public override string GenerateFileContent() => this.MockMethod.GetScriptContent();

    public override void Init() {
        if (!string.IsNullOrEmpty(this._scriptFile)) {
            this.Id = this._scriptFile;
            this.MockMethod.ImportScript(this._scriptFile);
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
        var id = AutoSave.getMockAutoSaveLocation(this.Io, this.MockMethod.MethodInfo, this.ServiceMock.Path);
        return id;
    }

    private void OnIsBusyChanged(ViewModelBase obj) => this.IsBusy = obj.IsBusy;
}