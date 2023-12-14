#region

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Tefin.Core.Interop;
using Tefin.Grpc;

#endregion

namespace Tefin.Features;

public class StartupFeature {
    public void Init() {
        foreach (Environment.SpecialFolder sf in Enum.GetValues(typeof(Environment.SpecialFolder))) {
            Console.WriteLine($"{sf} : {Environment.GetFolderPath(sf)}");
        }

        Startup.init();
        LiveCharts.Configure(config => {
            config.AddDarkTheme();
        });
    }

    public AppTypes.Root Load() {
        Core.App.init();
        return Core.App.loadRoot();
    }
}