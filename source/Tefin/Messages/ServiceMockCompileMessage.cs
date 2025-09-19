namespace Tefin.Messages;

public class ServiceMockCompileMessage(bool inprogress) : MessageBase {
    public bool InProgress { get; } = inprogress;
}