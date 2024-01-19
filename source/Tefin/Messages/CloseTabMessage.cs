#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public class CloseTabMessage(ITabViewModel tab) : TabMessageBase(tab);
public class CloseAllTabsMessage() : MessageBase();