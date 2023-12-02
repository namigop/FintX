#region

using Tefin.Core;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public static class TabFactory {

    public static ITabViewModel From(IExplorerItem item, IOResolver io) {
        switch (item) {
            case FilePerfNode p:
                return new FilePerfTabViewModel(p);

            case FileTestNode p2:
                return new FileTestTabViewModel(p2);

            case FileReqNode p3:
                return new FileReqTabViewModel(p3);

            case MethodNode p4:
                return new MethodTabViewModel(p4);

            default:
                io.Log.Warn($"Unable to open unknown item type: {item.GetType().FullName}");
                return default;
                //throw new NotSupportedException($"Unknown explorer item type: {item.GetType().FullName}");
        }
    }
}