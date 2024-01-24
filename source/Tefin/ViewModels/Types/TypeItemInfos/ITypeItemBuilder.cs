namespace Tefin.ViewModels.Types;

public interface ITypeItemBuilder {

    bool CanHandle(Type type);

    TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, bool generatePropertyNodes, object parentInstance, object instance,
        bool showNodeEditor);
}