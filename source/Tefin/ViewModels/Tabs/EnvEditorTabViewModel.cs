using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Config;

namespace Tefin.ViewModels.Tabs;

public class EnvEditorTabViewModel : PersistedTabViewModel {
    private const string _icon = "";
    private EnvDataViewModel _envData;
    private bool _isEditing;
    private readonly LoadEnvVarsFeature _loadEnv;
    private readonly string _envFile;

    public EnvEditorTabViewModel(EnvNode item) : base(item) {
        this._envFile =  Path.GetFullPath(item.EnvFile);
        this._envData = new EnvDataViewModel(item.GetEnvData());
        GlobalHub.subscribe<FileChangeMessage>(OnFileChange);
        _loadEnv = new LoadEnvVarsFeature();
    }

    private void OnFileChange(FileChangeMessage msg) {
        if (Path.GetFullPath(msg.FullPath) == this._envFile) {
            this.EnvData = this._loadEnv.LoadEnvVarsFromFile(this._envFile)
                .Then(t => new EnvDataViewModel(t));
        }
    }

    public EnvDataViewModel EnvData {
        get => this._envData;
        private set => this.RaiseAndSetIfChanged(ref  _envData , value);
    }

    public override string Icon => _icon;

    public override ProjectTypes.ClientGroup Client => ProjectTypes.ClientGroup.Empty();
    public override ClientMethodViewModelBase ClientMethod => null!;

    public bool IsEditing {
        get => this._isEditing;
        set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    public override string GenerateFileContent() {
        return this._isEditing ? "" : EnvData.GenerateFileContent();
    }

    public override void Init() {
        this.Id = this.GetTabId();
        this.Title = Path.GetFileName(this.Id);
    }

    public override void UpdateTitle(string oldFullPath, string newFullPath) => this.Title = Path.GetFileName(newFullPath);
    protected override string GetTabId() => ((EnvNode)this.ExplorerItem).FullPath;
}