#region

using Avalonia.Controls;

using Tefin.ViewModels;

#endregion

namespace Tefin.Views;

public partial class MainWindow : Window {

    public MainWindow() {
        this.InitializeComponent();
        this.DataContextChanged += (s, arg) => {
            var vm = (MainWindowViewModel)this.DataContext;
            vm.Init();
        };
    }
}