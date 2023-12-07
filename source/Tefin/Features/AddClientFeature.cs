using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class AddClientFeature {
    private readonly ProjectTypes.Project _project;
    private readonly string _clientName;
    private readonly string _serviceName;
    private readonly string _protoOrUrl;
    private readonly string _description;
    private readonly string[] _csFiles;
    private readonly IOResolver _io;
    public AddClientFeature(ProjectTypes.Project project, string clientName, string serviceName, string protoOrUrl, string description, string[] csFiles, IOResolver io) {
        this._project = project;
        this._clientName = clientName;
        this._serviceName = serviceName;
        this._protoOrUrl = protoOrUrl;
        this._description = description;
        this._csFiles = csFiles;
        this._io = io;
    }
    public async Task Add() {
        await Project.addClient(this._io, this._project, this._clientName, this._serviceName, this._protoOrUrl, this._description, this._csFiles);
    }
}