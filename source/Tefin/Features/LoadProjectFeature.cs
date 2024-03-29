﻿using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class LoadProjectFeature(IOs io, string projPath) {
    public ProjectTypes.Project Run() {
        var project = ProjectStructure.loadProject(io, projPath);
        var mon = new MonitorChangesFeature(io);
        mon.Run(project);
        return project;
    }
}