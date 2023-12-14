#region

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Tefin.Features;
using Tefin.ViewModels;
using Tefin.Views;

#endregion

namespace Tefin;

public class App : Application {
    public override void Initialize() {
        var start = new StartupFeature();
        start.Init();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow {
                DataContext = new MainWindowViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }

    private void AboutMenuItem_OnClick(object? sender, EventArgs e) {
    }
}