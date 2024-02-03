using Tefin.ViewModels.Tabs;

namespace Tefin.Messages;

public class OpenChildWindowMessage(ITabViewModel tab) : MessageBase {
    public ITabViewModel Content { get; } = tab;
}
public class CloseChildWindowMessage(ITabViewModel tab) : MessageBase {
    public ITabViewModel Content { get; } = tab;
}