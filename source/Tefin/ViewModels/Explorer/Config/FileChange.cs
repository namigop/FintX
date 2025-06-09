using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;
using Tefin.ViewModels.Explorer.Client;

namespace Tefin.ViewModels.Explorer.Config;

public static class FileChange<TRoot, TFile> where TRoot : ExplorerRootNode where TFile : FileNode {
    public static void Delete(IExplorerItem item, FileChangeMessage msg) {
        if (item is TFile node && node.FullPath == msg.FullPath) {
            node.Parent?.Items.Remove(node);
            RefreshClient(node);
        }
    }
    
    public static void FileModified(IExplorerItem item, FileChangeMessage msg) {
        if (item is TFile node && node.FullPath == msg.FullPath) {
            node.CheckGitStatus();
        }
    }

    private static void RefreshClient(IExplorerItem node) {
        var root = node.FindParentNode<TRoot>();
        var client = new LoadClientFeature(root!.Io, root.ClientPath).Run();
        GlobalHub.publish(new MessageProject.MsgClientUpdated(client, root.ClientPath, root.ClientPath));
    }

    public static void Rename(IExplorerItem item, FileChangeMessage msg) {
        var node = (TFile)item;
        if (node.FullPath == msg.OldFullPath) {
            node.UpdateFilePath(msg.FullPath);
            node.CheckGitStatus();
            if (node.Parent is FolderNode folderNode) {
                var path = Path.GetDirectoryName(msg.FullPath);
                if (folderNode.FullPath != path) {
                    //if the paths are not tha same then this node has the wrong parent node.
                    //this can happen because in Linux "moving" a file to another folder raises
                    //a single "rename" event instead of a "delete" followed by a "create" event
                    node.Parent.Items.Remove(node);
                    var root = node.FindParentNode<TFile>();
                    var newParent = root!.FindChildNode(i => i is FolderNode fn && fn.FullPath == path);
                    if (newParent != null) {
                        ((FolderNode)newParent).AddItem(node);
                    }
                }
            }


            RefreshClient(node);
        }
    }

    public static void Create(IExplorerItem item, FileChangeMessage msg, Func<string, TFile> newFileNode, Func<string, string> getRootPath) {
        var fileName = Path.GetFileName(msg.FullPath);
        
        if (item is TRoot root) {
            var rootPath = getRootPath(root.ClientPath);
            if (Path.Combine(rootPath, fileName) == msg.FullPath) {
                var ft = newFileNode(msg.FullPath);
                root.AddItem(ft);
                RefreshClient(ft);
                return;
            }
        }

        if (item is FolderNode node) {
            var dir = Path.GetDirectoryName(msg.FullPath);
            if (dir == node.FullPath) {
                var existing = node.Items.FirstOrDefault(t => t is TFile fn && fn.FullPath == msg.FullPath);
                if (existing == null) {
                    var n = newFileNode(msg.FullPath);
                    node.AddItem(n);
                    node.IsExpanded = true;
                    RefreshClient(n);
                }
            }
        }
    }
}