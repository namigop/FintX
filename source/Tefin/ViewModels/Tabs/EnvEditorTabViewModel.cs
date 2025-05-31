using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Config;

namespace Tefin.ViewModels.Tabs;

public class EnvEditorTabViewModel : PersistedTabViewModel {
    private const string _icon = "";
    private EnvDataViewModel _envData;
    private readonly EnvNode _envNode;
    private bool _isEditing;

    public EnvEditorTabViewModel(EnvNode item) : base(item) {
        this._envNode = item;
        this._envData = new EnvDataViewModel(item.GetEnvData());
        GlobalHub.subscribe<FileChangeMessage>(this.OnFileChanged).Then(this.MarkForCleanup);
    }

    private void OnFileChanged(FileChangeMessage obj) {
        Dispatcher.UIThread.Invoke(() => this.OnFileChangedInternal(obj));
    }

    private void OnFileChangedInternal(FileChangeMessage fileChangeMessage) {
        if (this._envNode.FullPath == fileChangeMessage.FullPath) {
            var d = VarsStructure.getVarsFromFile(this.Io, fileChangeMessage.FullPath);
            this.EnvData = new EnvDataViewModel(d);
        }
    }

    public EnvDataViewModel EnvData {
        get => this._envData;
        private set => this.RaiseAndSetIfChanged(ref  _envData , value);
    }

    public override string Icon => _icon;

    public override ProjectTypes.ClientGroup Client => ProjectTypes.ClientGroup.Empty();
    public override ClientMethodViewModelBase ClientMethod  => throw new NotImplementedException();

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