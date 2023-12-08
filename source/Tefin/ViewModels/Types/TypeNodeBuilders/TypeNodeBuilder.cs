namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public static class TypeNodeBuilder {
    private static readonly List<ITypeNodeBuilder> nodeBuilders = new();

    static TypeNodeBuilder() {
        nodeBuilders.Add(new CancellationTokenNodeBuilder());
        nodeBuilders.Add(new ByteStringNodeBuilder());
        nodeBuilders.Add(new ByteArrayNodeBuilder());
        // nodeBuilders.Add(new StreamNodeBuilder());
        nodeBuilders.Add(new DictionaryNodeBuilder());
        nodeBuilders.Add(new ArrayNodeBuilder());
        nodeBuilders.Add(new EnumNodeBuilder());
        // nodeBuilders.Add(new ObjectBottomNodeBuilder());
        nodeBuilders.Add(new SystemNodeBuilder());
        nodeBuilders.Add(new MetadataNodeBuilder());
        nodeBuilders.Add(new MetadataEntryNodeBuilder());
        nodeBuilders.Add(new TimestampNodeBuilder());
        nodeBuilders.Add(new ListNodeBuilder());

        // nodeBuilders.Add(new InheritanceNodeBuilder());
        // nodeBuilders.Add(new InterfaceNodeBuilder());
        nodeBuilders.Add(new DefaultNodeBuilder()); //always the last one
    }

    public static TypeBaseNode Create(string name, object instance) {
        return Create(name, instance.GetType(), null, new Dictionary<string, int>(), instance, null);
    }

    public static TypeBaseNode Create(string name, Type type, ITypeInfo? propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        foreach (var builder in nodeBuilders)
            if (builder.CanHandle(type))
                return builder.Handle(name, type, propInfo, processedTypeNames, instance, parent);

        throw new NotSupportedException($"Unable to build a node for {type.FullName}");
    }
}