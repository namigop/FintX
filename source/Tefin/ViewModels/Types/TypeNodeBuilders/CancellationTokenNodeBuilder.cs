#region

using System.Threading;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class CancellationTokenNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return type == typeof(CancellationToken);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode parent) {
        CancellationTokenNode t = new(name, type, propInfo, instance, parent);
        return t;
    }
}