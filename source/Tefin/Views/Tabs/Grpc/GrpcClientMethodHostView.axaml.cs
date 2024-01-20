#region

using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.ViewModels.Tabs.Grpc;

#endregion

namespace Tefin.Views.Tabs.Grpc;

public partial class GrpcClientMethodHostView : UserControl {

    public GrpcClientMethodHostView() {
        this.InitializeComponent();
        this.Loaded += this.OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) {
        var vm = this.DataContext as GrpcClientMethodHostViewModel;
        vm!.Init();
    }
}