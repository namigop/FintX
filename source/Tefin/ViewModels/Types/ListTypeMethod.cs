#region

using System.Linq;
using System.Reflection;

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class ListTypeMethod {
    private static readonly List<ListTypeMethod> Cache = new();
    private readonly Type _type;

    private ListTypeMethod(Type type) {
        this._type = type;
        this.TrySetupReflectionMethods();
    }

    public MethodInfo? AddMethod { get; private set; }
    public MethodInfo? AddRangeMethod { get; private set; }
    public MethodInfo? ClearMethod { get; private set; }
    public MethodInfo? GetEnumeratorMethod { get; private set; }
    public MethodInfo? RemoveAtMethod { get; private set; }
    public Type Type {
        get => this._type;
    }

    public static ListTypeMethod GetMethods(Type listType) {
        var i = Cache.FirstOrDefault(t => t.Type == listType);
        if (i != null) return i;

        ListTypeMethod? m = new(listType);
        Cache.Add(m);
        return m;
    }

    private void TrySetupReflectionMethods() {
        if (this.ClearMethod == null) {
            var type = this._type;
            this.ClearMethod = type.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
            this.AddRangeMethod = type.GetMethod("AddRange", BindingFlags.Public | BindingFlags.Instance);
            this.GetEnumeratorMethod = type.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
            var itemType = TypeHelper.getListItemType(type).Value;
            this.AddMethod = type.GetMethod("Add", new[] {
                itemType
            });
            this.RemoveAtMethod = type.GetMethod("RemoveAt", BindingFlags.Public | BindingFlags.Instance);
        }
    }
}