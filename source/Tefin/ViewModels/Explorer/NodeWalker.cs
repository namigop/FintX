using Tefin.Messages;

namespace Tefin.ViewModels.Explorer;

public static class NodeWalker {
    private static readonly object key = new();

    //walks through all the tree nodes
    public static void Walk(IExplorerItem[] items, FileChangeMessage msg, Action<IExplorerItem, FileChangeMessage> doAction, Func<IExplorerItem, bool> check) {
        lock (key) {
            var item = items.FirstOrDefault();
            if (item == null) {
                return;
            }

            if (check(item)) {
                doAction(item, msg);
            }

            Walk(item.Items.ToArray(), msg, doAction, check);
            Walk(items.Skip(1).ToArray(), msg, doAction, check);
        }
    }

    public static void Walk(IExplorerItem[] items, Action<IExplorerItem> doAction, Func<IExplorerItem, bool> check) {
        lock (key) {
            var item = items.FirstOrDefault();
            if (item == null) {
                return;
            }

            if (check(item)) {
                doAction(item);
            }

            Walk(item.Items.ToArray(), doAction, check);
            Walk(items.Skip(1).ToArray(), doAction, check);
        }
    }
}