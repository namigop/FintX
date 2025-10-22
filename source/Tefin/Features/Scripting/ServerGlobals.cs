using Grpc.Core;

using Tefin.Core;

namespace Tefin.Features.Scripting;

public class ServerGlobals {

    private readonly object? _requestStream;
    private readonly object? _responseStream;
    private readonly IOs _io;

    public ServerGlobals(object? request, object? requestStream, object? responseStream, ServerCallContext context, IOs io) {
        this._requestStream = requestStream;
        this._responseStream = responseStream;
        this.context = context;
        this._io = io;
    }

    // ReSharper disable once InconsistentNaming
    public Dictionary<string, object> variables { get; } = new();
    // ReSharper disable once InconsistentNaming
    public ScriptUtilJson json { get; } = new ();
    // ReSharper disable once InconsistentNaming
    public ServerCallContext context { get; }

    // ReSharper disable once InconsistentNaming
    public Log.ILog log => this._io.Log;
    
    // ReSharper disable once InconsistentNaming
    public Task sleep(int ms) => Task.Delay(ms);

     
    public class ScriptUtilJson {
        
        // ReSharper disable once UnusedMember.Global
        public string From(object? obj) {
            if (obj == null) {
                return "{}";
            }

            return Instance.indirectSerialize(obj.GetType(), obj);
        }
    
        // ReSharper disable once UnusedMember.Global
        public string Get(string jPath, string json) {
            ArgumentNullException.ThrowIfNull(jPath);
            var res = Tefin.Core.Instance.jsonSelectToken(jPath, json);
            if (res.IsOk)
                return res.ResultValue.ToObject<string>() ?? "<null>";
        
            return $"<error> {jPath} did not match any node in the json";
        }
    }
}