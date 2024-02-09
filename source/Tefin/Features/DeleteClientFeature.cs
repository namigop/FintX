#region

using Tefin.Core;
using Tefin.Core.Interop;

#endregion

namespace Tefin.Features;

public class DeleteClientFeature(ProjectTypes.ClientGroup client, IOs io) {
    public void Delete() => ClientStructure.deleteClient(client, io);
}