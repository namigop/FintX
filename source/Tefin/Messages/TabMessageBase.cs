#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public abstract class TabMessageBase(ITabViewModel tab) : MessageBase {
    public ITabViewModel Tab { get; } = tab;
}