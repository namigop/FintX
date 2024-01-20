#region

#endregion

namespace Tefin.Messages;

public class ClientCompileMessage(bool inprogress) : MessageBase {
    public bool InProgress { get; } = inprogress;
}