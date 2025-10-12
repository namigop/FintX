using Avalonia.Threading;

using Newtonsoft.Json.Linq;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Explorer.ServiceMock;
using Tefin.ViewModels.Tabs;

namespace Tefin.Features;

public class LoadScriptSessionFeature(
    string mockPath,
    IEnumerable<MockMethodNode> nodes,
    IOs io,
    Action<bool> onLoaded) {
    private bool IsAutoSaveFile(string scriptFile) {
        var dir = Path.GetDirectoryName(scriptFile);
        return dir != null && dir.EndsWith(ServiceMockStructure.AutoSaveFolderName);
    }

    private void LoadOne(string json, string scriptFile) {
        var ext = Path.GetExtension(scriptFile);
        if (ext != Ext.mockScriptExt)
            return;
        
        var methodName = Core.Utils.jSelectToken(json, "$.Method").Value<string>();
        var item = nodes.FirstOrDefault(i => i.MethodInfo.Name == methodName);
        if (item != null) {
            IExplorerItem? node = item;
            var isAutoSave = this.IsAutoSaveFile(scriptFile);
            if (!isAutoSave) {
                //if its not an auto-save file, open it as an existing file request
                var fileNode = item.Items.FirstOrDefault(c => ((FileNode)c).FullPath == scriptFile);
                node = fileNode;
            }

            if (node != null) {
                var tab = TabFactory.From(node, io, scriptFile);
                if (tab != null) {
                    GlobalHub.publish(new OpenTabMessage(tab));
                }
            }
        }
    }

    private Action CreateAction(string scriptFile) {
        var json = io.File.ReadAllText(scriptFile);
        if (string.IsNullOrWhiteSpace(json)) {
            return () => { };
        }

        return () => this.LoadOne(json, scriptFile);
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
        var projectPath = Path.GetDirectoryName(mockPath)
                .Then(p => Path.Combine(p, "../"))
                .Then(Path.GetFullPath);
        var state = ProjectStructure.getSaveState(io, projectPath);
        var openFiles = state.MockState.SelectMany(c => c.OpenScripts)
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