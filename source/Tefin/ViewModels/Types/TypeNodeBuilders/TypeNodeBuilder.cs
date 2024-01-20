#region

using Tefin.Core;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public static class TypeNodeBuilder {
    private static readonly List<ITypeNodeBuilder> NodeBuilders = new();

    static TypeNodeBuilder() {
        NodeBuilders.Add(new CancellationTokenNodeBuilder());
        NodeBuilders.Add(new ExceptionNodeBuilder());
        NodeBuilders.Add(new ByteStringNodeBuilder());
        NodeBuilders.Add(new ByteArrayNodeBuilder());
        // nodeBuilders.Add(new StreamNodeBuilder());
        NodeBuilders.Add(new DictionaryNodeBuilder());
        NodeBuilders.Add(new ArrayNodeBuilder());
        NodeBuilders.Add(new EnumNodeBuilder());
        // nodeBuilders.Add(new ObjectBottomNodeBuilder());
        NodeBuilders.Add(new SystemNodeBuilder());
        NodeBuilders.Add(new MetadataNodeBuilder());
        NodeBuilders.Add(new MetadataEntryNodeBuilder());
        NodeBuilders.Add(new TimestampNodeBuilder());
        NodeBuilders.Add(new ListNodeBuilder());

        // nodeBuilders.Add(new InheritanceNodeBuilder());
        // nodeBuilders.Add(new InterfaceNodeBuilder());

        NodeBuilders.Add(new DefaultNodeBuilder()); //always the last one
    }

    public static TypeBaseNode Create(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        foreach (var builder in NodeBuilders)
            if (builder.CanHandle(type)) {
                try {
                    return builder.Handle(name, type, propInfo, processedTypeNames, instance, parent);
                }
                catch (Exception exc) {
                    Resolver.value.Log.Warn(exc.Message);
                }
            }

        throw new NotSupportedException($"Unable to build a node for {type.FullName}");
    }
}