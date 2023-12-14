#region

using Google.Protobuf.WellKnownTypes;

using Type = System.Type;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class TimestampNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) {
        return type == typeof(Timestamp);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        TimestampNode? t = new(name, type, propInfo, instance, parent);
        return t;
    }
}