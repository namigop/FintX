#region

#endregion

using Tefin.ViewModels.Explorer;

namespace Tefin.Messages;

public class ClientCompileMessage(bool inprogress) : MessageBase {
    public bool InProgress { get; } = inprogress;
}

public class CutCopyNodeMessage(string[] pathsToCopy, NodeBase[] nodes, bool isFile, bool isCutOperation = false) : MessageBase {
    public NodeBase[] Nodes { get; private set; } = nodes;
    public bool IsFile { get; private set; } = isFile;
    public string[] PathsToCopy { get; private set; } = pathsToCopy;
    public bool IsCut { get; private set; } = isCutOperation;
}

public class PasteNodeMessage(NodeBase source) : MessageBase {
    public NodeBase Source { get; init; } = source;
}