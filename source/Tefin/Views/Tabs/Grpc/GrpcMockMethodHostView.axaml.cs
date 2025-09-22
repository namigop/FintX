#region

using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.ViewModels.Tabs.Grpc;

#endregion

namespace Tefin.Views.Tabs.Grpc;

public partial class GrpcMockMethodHostView : UserControl {
    public GrpcMockMethodHostView() {
        this.InitializeComponent();
        this.Loaded += this.OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) {
        var vm = this.DataContext as GrpcMockMethodHostViewModel;
        vm!.Init();
    }
}