#region

using Tefin.Core.Infra.Actors;

#endregion

namespace Tefin.Messages;

public abstract class MessageBase : Hub.IMsg {
    public string Id { get; } = DateTime.Now.ToString("yyyy-M-d HH:mm:ss.fff");
}