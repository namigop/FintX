namespace Tefin.Messages;

public class MethodCallMessage(string url, string method, double point) : MessageBase {
    public string Method { get; } = method;
    public double Point { get; } = point;
    public string Url { get; } = url;
}