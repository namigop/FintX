#region

using Tefin.Core;
using Tefin.Core.Interop;

#endregion

namespace Tefin.Features;

public class DeleteClientFeature(ProjectTypes.ClientGroup client, IOResolver io) {
    public void Delete() {
        Project.deleteClient(client, io);
    }
}