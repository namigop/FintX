#region

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc;

#endregion

namespace Tefin.Features;

public class StartupFeature {
    public void Init() {
        //GenericServiceImplementationGenerator.Test();

        Startup.init();
        Current.Init();
        LiveCharts.Configure(config => {
            config.AddDarkTheme();
        });
    }

    public AppTypes.Root Load(IOs io) {
        Core.App.init(io);
        var root = Core.App.loadRoot(io);
        var proj = root.Packages.First().Projects.First();
        var mon = new MonitorChangesFeature(io);
        mon.Run(proj);

        return root;
    }
}