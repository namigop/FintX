using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Tefin.Core.Interop;

namespace Tefin.Features;

public class StartupFeature() {

    public void Init() {
        Core.App.init();
        Grpc.Startup.init();
        LiveCharts.Configure(config => {
            config.AddDarkTheme();
        });
    }

    public AppTypes.Root Load() {
        return Core.App.loadRoot();
    }
}