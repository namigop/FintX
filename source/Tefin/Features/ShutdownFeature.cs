using Tefin.Core.Interop;
using Tefin.ViewModels;

namespace Tefin.Features;

public class ShutdownFeature(MainWindowViewModel main) {
    public void Run() {
        var projects = main.ProjectMenuViewModel.RecentProjects
            .Select(p => AppTypes.AppProject.Create(p.Path, p.Package))
            .ToArray();
        
        var activeProject =  main.ProjectMenuViewModel.RecentProjects.First(p => p.IsSelected);
        var appProject = AppTypes.AppProject.Create(activeProject.Path, activeProject.Package);
        Core.App.saveAppState(main.Io, projects, appProject);
    }
}