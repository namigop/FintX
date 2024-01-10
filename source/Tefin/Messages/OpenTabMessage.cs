#region

using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Messages;

public class OpenTabMessage(ITabViewModel tab) : TabMessageBase(tab);