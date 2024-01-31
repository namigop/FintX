#region

using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ClientSubMenuViewModel : ViewModelBase, ISubMenusViewModel {
    private readonly ExplorerViewModel _explorerViewModel;

    public ClientSubMenuViewModel(ExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.AddClientCommand = this.CreateCommand(this.OnAddClient);
        this.ImportCommand = this.CreateCommand(this.OnImport);
    }

    private async Task OnImport() {
        var fileExtensions = new[] { $"*{Ext.zipExt}" };
        var fileTitle = "FintX (*.zip)";
        var (ok, files) = await DialogUtils.OpenFile("Open zip file", fileTitle, fileExtensions);
        if (ok) {
            var zipFile = files[0];
            var updated = Share.importInto(this.Io, this._explorerViewModel.Project, zipFile);
            this._explorerViewModel.GetClientNodes().FirstOrDefault(f => f.Client. )
        }
    }

    public ICommand AddClientCommand { get; }
    public ICommand ImportCommand { get; }
    

    private void OnAddClient() {
        AddGrpcServiceOverlayViewModel overlayVm = new(this._explorerViewModel.Project!);
        OpenOverlayMessage msg = new(overlayVm);
        GlobalHub.publish(msg);
    }

   
}