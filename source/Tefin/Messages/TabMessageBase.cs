#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public abstract class TabMessageBase : MessageBase {
    protected TabMessageBase(ITabViewModel tab) {
        this.Tab = tab;
    }

    public ITabViewModel Tab { get; }
}