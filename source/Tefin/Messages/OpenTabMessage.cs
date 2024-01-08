#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public class OpenTabMessage : TabMessageBase {
    public OpenTabMessage(ITabViewModel tab, string file) : base(tab) {
        this.RequestFile = file;
    }

    public string RequestFile {
        get;
    }
}