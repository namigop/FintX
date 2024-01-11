using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Tabs;

namespace Tefin.ViewModels.Explorer;

public class FileReqNode : FileNode {
    public FileReqNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
        this.OpenCommand = CreateCommand(this.OnOpen);
        this.DeleteCommand = CreateCommand(this.OnDelete);
    }

    private void OnDelete() {
        // Delete from the file system *only*.  The OS event notification will
        // get handled by ExplorerViewModel and the delete file will be removed
        // from the explorer tree
         Io.File.Delete(this.FullPath);
    }

    public ICommand OpenCommand { get; }

    public ICommand DeleteCommand { get; }

    public ClientMethodViewModelBase? CreateViewModel() {
        return ((MethodNode)this.Parent).CreateViewModel();
    }
    private void OnOpen() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null)
            GlobalHub.publish(new OpenTabMessage(tab));
    }

}