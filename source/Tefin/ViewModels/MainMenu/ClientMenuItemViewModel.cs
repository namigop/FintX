#region

using System.Reactive;

using Tefin.ViewModels.Explorer;

using static Tefin.Core.Interop.ProjectTypes;

#endregion

namespace Tefin.ViewModels.MainMenu;

//Single main menu item

//sub menus of the menu item

public class ClientMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public ClientMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.Explorer = new ExplorerViewModel();
        this.SubMenus = new ClientSubMenuViewModel(this.Explorer);
    }

    public ExplorerViewModel Explorer {
        get;
    }

    public Project Project {
        get => this.Explorer.Project!;
        private set => this.Explorer.Project = value;
    }

    public override string ToolTip { get; } = "View clients";

    public override string Name { get; } = "Clients";

    public override string ShortName { get; } = "clients";
    public override ISubMenusViewModel? SubMenus { get; }

    public void Init(Project proj) {
        this.Project = proj;
        foreach (var client in proj.Clients) {
            //Create the client node but do not recompile the client
            this.Explorer.AddClientNode(client);
        }
    }

    protected override void OnSelectItem() {
        base.OnSelectItem();
        if (!this.Explorer.ExplorerTree.Items.Any()) {
            //if empty show the add client screen
            var sub = (ClientSubMenuViewModel)this.SubMenus!;
            sub.AddClientCommand.Execute(Unit.Default);
        }
    }
}