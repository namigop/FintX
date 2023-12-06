#region

using Grpc.Core;

using Tefin.ViewModels.Types.TypeNodeBuilders;

#endregion

namespace Tefin.ViewModels.Types;

public class MetadataEntryNode : TypeBaseNode {
    private readonly Metadata.Entry _entry;
    private readonly object _index;
    private readonly Metadata _metadata;

    public MetadataEntryNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(name, type, propInfo, instance, parent) {
        this._metadata = (Metadata)parent!.Value!;
        this._entry = (Metadata.Entry)instance!;
        this._index = this._metadata.IndexOf(this._entry);
        this.Title = $"Item[{this._index}]";
        this.Key = "";
    }

    public override string FormattedValue => this._entry.Value;

    public string Key { get; set; }
    public int TargetListItemsCount { get; set; }

    public override void Init(Dictionary<string, int> processedTypeNames) {
        this.Key = this._entry.Key;

        //Create child nodes Key and Value that when edited will change the Key and EntryValue properties of this
        //MetadataEntryNode.  This is different from how the other nodes work!
        TypeInfo? keyPropInfo = new(typeof(MetadataEntryNode).GetProperty("Key")!);
        var keyNode = TypeNodeBuilder.Create("Key", typeof(string), keyPropInfo, new Dictionary<string, int>(), this.Key, this);
        TypeInfo? entryValuePropInfo = new(typeof(MetadataEntryNode).GetProperty(nameof(this.FormattedValue))!);
        var valueNode = TypeNodeBuilder.Create("Value", typeof(string), entryValuePropInfo, new Dictionary<string, int>(), this.FormattedValue, this);

        this.Items.Add(keyNode);
        this.Items.Add(valueNode);

        //keyNode.CanShowLock = true;
        //valueNode.CanShowLock = true;

        keyNode.Init();
        valueNode.Init();
    }

    protected override void OnValueChanged(object? oldValue, object? newValue) {
    }
}