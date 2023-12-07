using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Tefin.Core.Interop;

namespace Tefin.Features;

public class StartupFeature {
    public StartupFeature() {
    }

    public void Init() {
       
        foreach (Environment.SpecialFolder sf in Enum.GetValues(typeof(System.Environment.SpecialFolder)))
        {
            Console.WriteLine( $"{sf} : {Environment.GetFolderPath(sf)}");
        }
        
        Grpc.Startup.init();
        LiveCharts.Configure(config => {
            config.AddDarkTheme();
        });
    }

    public AppTypes.Root Load() {
        Core.App.init();
        return Core.App.loadRoot();
    }
}