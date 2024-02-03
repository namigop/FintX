using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class LoadProjectFeature(IOResolver io, string projPath) {
    public ProjectTypes.Project Run() {
        var project = Project.loadProject(io, projPath);
        var mon = new MonitorChangesFeature(io);
        mon.Run(project);
        return project;
    }
}