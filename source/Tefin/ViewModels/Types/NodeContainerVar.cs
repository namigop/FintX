namespace Tefin.ViewModels.Types;

public class NodeContainerVar(List<VarDefinition> requestVariables, string clientPath) {
    public string ClientPath { get; set; } = clientPath;
    public List<VarDefinition> Variables { get; set; } = requestVariables;

    public static NodeContainerVar FromMethodInfoNode(MethodInfoNode m) => new(m.Variables, m.ClientGroup.Path);

    public static NodeContainerVar FromResponseNode(ResponseNode m) => new(m.Variables, m.ClientPath);

    public static NodeContainerVar FromResponseStreamNode(StreamNode rs) => new(rs.Variables, rs.ClientPath);
}