#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public class CloseTabMessage : TabMessageBase {

    public CloseTabMessage(ITabViewModel tab) : base(tab) {
    }
}