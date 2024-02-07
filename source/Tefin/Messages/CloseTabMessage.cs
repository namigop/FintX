#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public class CloseTabMessage(ITabViewModel tab) : TabMessageBase(tab);

public class RemoveTabMessage(ITabViewModel tab) : TabMessageBase(tab);

public class CloseAllTabsMessage : MessageBase;

public class CloseAllOtherTabsMessage(ITabViewModel tab) : TabMessageBase(tab);