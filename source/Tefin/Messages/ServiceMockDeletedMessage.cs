using Tefin.Core.Interop;

namespace Tefin.Messages;

public class ServiceMockDeletedMessage(ProjectTypes.ServiceMockGroup mock) : MessageBase {
    public ProjectTypes.ServiceMockGroup ServiceMock { get; } = mock;
}