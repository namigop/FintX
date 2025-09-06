using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class LoadClientFeature(IOs io, string clientPath) {
    public ProjectTypes.ClientGroup Run() => ClientStructure.loadClient(io, clientPath);
}