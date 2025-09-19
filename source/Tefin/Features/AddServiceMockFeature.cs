using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class AddServiceMockFeature(
    ProjectTypes.Project project,
    string serviceName,
    string protoOrUrl,
    string description,
    string[] csFiles,
    string dll,
    IOs io) {
    public void Add() {
        ServiceMockStructure.addMock(io, project, serviceName, protoOrUrl, description, csFiles, dll);
    }
}