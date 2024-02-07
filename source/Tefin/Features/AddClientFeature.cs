#region

using Tefin.Core;
using Tefin.Core.Interop;

#endregion

namespace Tefin.Features;

public class AddClientFeature(
    ProjectTypes.Project project,
    string clientName,
    string serviceName,
    string protoOrUrl,
    string description,
    string[] csFiles,
    IOs io) {
    public async Task Add() =>
        await Project.addClient(io, project, clientName, serviceName, protoOrUrl, description, csFiles);
}