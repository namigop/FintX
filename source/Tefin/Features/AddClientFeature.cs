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
    string dll,
    IOs io) {
    public async Task Add() =>
        await ClientStructure.addClient(io, project, clientName, serviceName, protoOrUrl, description, csFiles, dll);
}