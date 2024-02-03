using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Tabs;

namespace Tefin.ViewModels;

public class ChildWindowViewModel : ViewModelBase {
    public ChildWindowViewModel(ITabViewModel content) {
        this.Content = content;

        this.DockCommand = this.CreateCommand(this.OnDock);
        //todo;rename/delete from explorer tree
    }

    public Action WindowClose { get; set; }

    public ITabViewModel Content { get; }
    public ICommand DockCommand { get; }

    private void OnDock() {
        this.WindowClose();
        GlobalHub.publish(new OpenTabMessage(this.Content));
    }
}