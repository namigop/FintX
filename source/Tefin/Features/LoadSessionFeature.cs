using Avalonia.Threading;

using Newtonsoft.Json.Linq;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Tabs;

namespace Tefin.Features;

public class LoadSessionFeature(
    string clientPath,
    IEnumerable<MethodNode> nodes,
    IOs io,
    Action<bool> onLoaded) {
    private bool IsAutoSaveFile(string reqFile) {
        var dir = Path.GetDirectoryName(reqFile);
        return dir != null && dir.EndsWith(ClientStructure.AutoSaveFolderName);
    }

    private void LoadOne(string json, string reqFile) {
        var ext = Path.GetExtension(reqFile);
        if (ext != Ext.requestFileExt)
            return;
        
        var methodName = Core.Utils.jSelectToken(json, "$.Method").Value<string>();
        var item = nodes.FirstOrDefault(i => i.MethodInfo.Name == methodName);
        if (item != null) {
            IExplorerItem? node = item;
            var isAutoSave = this.IsAutoSaveFile(reqFile);
            if (!isAutoSave) {
                //if its not an auto-save file, open it as an existing file request
                var fileNode = item.Items.FirstOrDefault(c => ((FileReqNode)c).FullPath == reqFile);
                node = fileNode;
            }

            if (node != null) {
                var tab = TabFactory.From(node, io, reqFile);
                if (tab != null) {
                    GlobalHub.publish(new OpenTabMessage(tab));
                }
            }
        }
    }

    private Action CreateAction(string reqFile) {
        var json = io.File.ReadAllText(reqFile);
        if (string.IsNullOrWhiteSpace(json)) {
            return () => { };
        }

        return () => this.LoadOne(json, reqFile);
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
            TimeSpan.FromMilliseconds(50));
    }

    public void Run() {
        var projectPath = Path.GetDirectoryName(clientPath);
        var state = ProjectStructure.getSaveState(io, projectPath);
        var openFiles = state.ClientState.SelectMany(c => c.OpenFiles)
            .Where(io.File.Exists)
            .OrderBy(c => c)
            .ToArray();

        openFiles
            .Select(this.CreateAction)
            .ToArray()
            .Then(actions => {
                if (actions.Length != 0) {
                    this.ExecuteActions(actions);
                }
                else {
                    onLoaded(true);
                }
            });
    }
}