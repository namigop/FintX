using Avalonia.Threading;
using Newtonsoft.Json.Linq;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Tabs;

namespace Tefin.Features;

public class LoadSessionFeature(string clientPath, IEnumerable<MethodNode> nodes, IOResolver io, Action<bool> onLoaded) {
    private void LoadOne(string json, string reqFile) {
        var methodName = Core.Utils.jSelectToken(json, "$.Method").Value<string>();
        var item = nodes.FirstOrDefault(i => i.MethodInfo.Name == methodName);
        if (item != null) {
            var tab = TabFactory.From(item, io, reqFile);
            if (tab != null)
                GlobalHub.publish(new OpenTabMessage(tab));
        }
    }
    private Action CreateAction(string reqFile) {
        var json = io.File.ReadAllText(reqFile);
        if (string.IsNullOrWhiteSpace(json))
            return null;
        return new Action(() => LoadOne(json, reqFile));
    }
    private void ExecuteActions(Action[] actions) {
        var pos = 0;
        DispatcherTimer.Run(
            () => {
                if (pos < actions.Length) {
                    actions[pos]!.Invoke();
                    pos += 1;
                    return true;
                }

                onLoaded(true);
                return false;
            },
            TimeSpan.FromMilliseconds(100));
    }
    public void Run() {
        AutoSave.getAutoSavedFiles(io, clientPath)
            .Select(CreateAction)
            .ToArray()
            .Then(actions => {
                if (actions.Length != 0) {
                    ExecuteActions(actions);
                }
                else {
                   onLoaded(true);
                }
            });
    }

}