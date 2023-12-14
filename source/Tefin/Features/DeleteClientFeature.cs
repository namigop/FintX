#region

using Tefin.Core;
using Tefin.Core.Interop;

#endregion

namespace Tefin.Features;

public class DeleteClientFeature {
    private readonly ProjectTypes.ClientGroup _client;
    private readonly IOResolver _io;
    public DeleteClientFeature(ProjectTypes.ClientGroup client, IOResolver io) {
        this._client = client;
        this._io = io;
    }
    public void Delete() {
        Project.deleteClient(this._client, this._io);
    }
}