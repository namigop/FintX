#region

using Avalonia.Controls;

using Tefin.ViewModels;

#endregion

namespace Tefin.Views;

public partial class MainWindow : Window {

    public MainWindow() {
        this.InitializeComponent();
        this.DataContextChanged += (s, arg) => {
            var vm = (MainWindowViewModel)this.DataContext!;
            try {
                vm.Init();
            }
            catch (Exception exc) {
                vm.Io.Log.Error(exc);
            }
        };
    }
}