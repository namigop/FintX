#region

global using System;
global using System.Collections.Generic;
global using System.Threading.Tasks;
global using System.IO;
global using System.Linq;

using Avalonia;
using Avalonia.ReactiveUI;

#endregion

namespace Tefin;

internal class Program {
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace().UseReactiveUI();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}