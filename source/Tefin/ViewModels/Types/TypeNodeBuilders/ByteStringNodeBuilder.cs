#region

using Google.Protobuf;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class ByteStringNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return type == typeof(ByteString);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        ByteStringNode t = new(name, type, propInfo, instance, parent);
        return t;
    }
}