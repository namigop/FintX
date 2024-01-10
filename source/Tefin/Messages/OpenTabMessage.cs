#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public class OpenTabMessage : TabMessageBase {
    public OpenTabMessage(ITabViewModel tab) : base(tab) {
    }
}