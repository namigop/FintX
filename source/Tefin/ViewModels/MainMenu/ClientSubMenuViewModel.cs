#region

using System.Reactive;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Features;
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
            var project = this._explorerViewModel.Project;
            var (clientName, updated) = Share.importInto(this.Io, project, zipFile);
            var clientNode = this._explorerViewModel.GetClientNodes().FirstOrDefault(f => f.Client.Name == clientName);
            if (updated) {
                if (clientNode == null) {
                    var loadProj = new LoadProjectFeature(this.Io, project!.Path);
                    project = loadProj.Run();
                        
                    var cg = project.Clients.First(t => t.Name == clientName);
                    clientNode = this._explorerViewModel.AddClientNode(cg);
                }

                if (!clientNode!.IsLoaded) {
                    clientNode.CompileClientTypeCommand.Execute(Unit.Default);
                }

            }
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