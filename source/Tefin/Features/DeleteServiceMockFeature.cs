using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class DeleteServiceMockFeature(ProjectTypes.ServiceMockGroup mock, IOs io) {
    public void Delete() => ServiceMockStructure.deleteMock(io, mock);
}