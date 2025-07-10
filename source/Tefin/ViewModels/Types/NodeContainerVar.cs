namespace Tefin.ViewModels.Types;

public class NodeContainerVar(List<VarDefinition> requestVariables, string clientPath) {
    public List<VarDefinition> Variables { get; set; } = requestVariables;
    public string ClientPath { get; set; } = clientPath;

    public static NodeContainerVar FromMethodInfoNode(MethodInfoNode m) {
        return new NodeContainerVar(m.Variables, m.ClientGroup.Path);
    }
    public static NodeContainerVar FromResponseNode(ResponseNode m) {
        return new NodeContainerVar(m.Variables, m.ClientPath);
    }

    public static NodeContainerVar FromResponseStreamNode(StreamNode rs) {
        return new NodeContainerVar(rs.Variables, rs.ClientPath);
    }
}