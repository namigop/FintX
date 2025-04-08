using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

using Tefin.ViewModels.Types;

namespace Tefin.Views.ProjectEnv;

public partial class NewEnvVariableContext : UserControl {
    public NewEnvVariableContext() {
        InitializeComponent();
        
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        var vm = this.DataContext as EnvVarNodeViewModel;
        if (string.IsNullOrEmpty(vm?.EnvVarTag)) {
            vm?.Reset();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        var vm = this.DataContext as EnvVarNodeViewModel;
        if (string.IsNullOrEmpty(vm?.EnvVarTag)) {
            vm?.ShowDefault();
        }
    }

    private void ButtonOkayClick(object? sender, RoutedEventArgs e) {
        var vm = this.DataContext as EnvVarNodeViewModel;
        vm?.CreateEnvVariableCommand.Execute(null);
        this.ClosePopup();
    }

    private void ClosePopup() {
        var pop = this.FindLogicalAncestorOfType<Popup>();
        if (pop != null) {
            pop.IsOpen = false;
        }
    }

    private void ButtonCancelClick(object? sender, RoutedEventArgs e) {
        this.ClosePopup();
        var vm = this.DataContext as EnvVarNodeViewModel;
        vm?.Reset();
    }
}