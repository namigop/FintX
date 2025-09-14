using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class DeleteServiceMockFeature(ProjectTypes.ServiceMockGroup client, IOs io) {
    public void Delete() => throw new NotImplementedException(); //ClientStructure.deleteClient(client, io);
}