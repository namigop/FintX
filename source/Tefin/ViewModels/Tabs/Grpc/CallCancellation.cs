#region

using System.Threading;

using Grpc.Core;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

/// <summary>
/// Tells a user initiated stop apart from a call that failed.
/// </summary>
public static class CallCancellation {
    /// <summary>
    /// True when the exception is the result of the user stopping the call.
    /// </summary>
    /// <param name="error">The exception that ended the call.</param>
    /// <param name="callToken">The token of the call.</param>
    public static bool IsUserCancellation(Exception? error, CancellationToken callToken) {
        if (!callToken.IsCancellationRequested) {
            //the user did not stop the call, so whatever went wrong is a real failure
            return false;
        }

        return error is OperationCanceledException
               || error is RpcException { StatusCode: StatusCode.Cancelled };
    }
}
