#region

using System.Reflection;

using Google.Protobuf.Collections;

#endregion

namespace Tefin.ViewModels.Types;

public class DictionaryNode : ListNode {
    //private int listItemsCount;
    private readonly object _internalList;

    private readonly Type _internalListType;

    private readonly Type _itemType;

    private readonly ListTypeMethod _listMethods;
    //private readonly ListTypeMethod _listMethods;

    public DictionaryNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(
        name, type, propInfo, instance, parent) {
        var listType = typeof(ListOfPairs<,>);
        var typeArgs = type.GetGenericArguments();
        var constructedListType = listType.MakeGenericType(typeArgs);
        this._listMethods = ListTypeMethod.GetMethods(constructedListType);
        this._itemType = typeof(Pair<,>).MakeGenericType(typeArgs);
        this._internalList =
            ToListOfPairs(constructedListType, instance)!; // Activator.CreateInstance(constructedListType);
        this._internalListType = constructedListType;

        this.FormattedTypeName = $"{{dict<{typeArgs[0].Name},{typeArgs[1].Name}>}}";
        //this._toDictionary = _internalListType.GetMethod("ToDictionary", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    }

    public override string FormattedTypeName { get; }

    public static object? ToListOfPairs(Type lpType, object? dict) {
        if (dict == null) {
            return null;
        }

        var lpInstance = Activator.CreateInstance(lpType);
        var fromDictionary = lpType.GetMethod("FromDictionary", BindingFlags.Public | BindingFlags.Instance);
        fromDictionary!.Invoke(lpInstance, [dict]);
        return lpInstance;
    }

    protected override Type GetItemType() => this._itemType;

    protected override object GetListInstance() => this._internalList;

    protected override ListTypeMethod GetMethods() => this._listMethods;

    public class ListOfPairs<K, V> : List<Pair<K, V>> where K : notnull {
        public void FromDictionary(Dictionary<K, V> source) {
            this.Clear();
            foreach (var i in source) {
                this.Add(new Pair<K, V>(i.Key, i.Value));
            }
        }

        public void FromMapField(MapField<K, V> source) {
            this.Clear();
            foreach (var i in source) {
                this.Add(new Pair<K, V>(i.Key, i.Value));
            }
        }

        public IDictionary<K, V> ToDictionary() {
            var d = new Dictionary<K, V>();
            foreach (var k in this) {
                d[k.Key] = k.Value;
            }

            return d;
        }

        public MapField<K, V> ToMapField() {
            var d = new MapField<K, V>();
            foreach (var k in this) {
                d[k.Key] = k.Value;
            }

            return d;
        }
    }

    public class Pair<K, V>(K key, V value) {
        public K Key { get; } = key;
        public V Value { get; } = value;
    }
}