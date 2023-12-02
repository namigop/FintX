namespace Tefin.Messages;

public class MethodCallMessage : MessageBase {

    public MethodCallMessage(string url, string method, double point) {
        this.Url = url;
        this.Method = method;
        this.Point = point;
    }

    public string Method { get; }
    public double Point { get; }
    public string Url { get; }
}