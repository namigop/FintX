#region

using System.Text;
using System.Threading;

using Microsoft.FSharp.Core;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Types.TypeEditors;

#endregion

namespace Tefin.ViewModels.Types;

public class SystemNode : TypeBaseNode {
   
    public SystemNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(name,
        type, propInfo, instance, parent) {
        this.EnvVar = new(this);
       
        this.FormattedTypeName = $"{{{SystemType.getDisplayName(type)}}}";
        if (type == typeof(string)) {
            this.Editor = new StringEditor(this);
        }
        else if (type == typeof(int)) {
            this.Editor = new IntEditor(this);
        }
        else if (type == typeof(short)) {
            this.Editor = new Int16Editor(this);
        }
        else if (type == typeof(long)) {
            this.Editor = new Int64Editor(this);
        }
        else if (type == typeof(uint)) {
            this.Editor = new UIntEditor(this);
        }
        else if (type == typeof(ushort)) {
            this.Editor = new UInt16Editor(this);
        }
        else if (type == typeof(ulong)) {
            this.Editor = new UInt64Editor(this);
        }
        else if (type == typeof(decimal)) {
            this.Editor = new DecimalEditor(this);
        }
        else if (type == typeof(double)) {
            this.Editor = new DoubleEditor(this);
        }
        else if (type == typeof(float)) {
            this.Editor = new Float32Editor(this);
        }
        else if (type == typeof(bool)) {
            this.Editor = new BoolEditor(this);
        }
        else if (type == typeof(DateTime)) {
            this.Editor = new DateTimeEditor(this);
        }
        else if (type == typeof(DateTimeOffset)) {
            this.Editor = new DateTimeOffsetEditor(this);
        }
        else if (type == typeof(Guid)) {
            this.Editor = new GuidEditor(this);
        }
        else if (type == typeof(TimeSpan)) {
            this.Editor = new TimeSpanEditor(this);
        }
        else if (type == typeof(CancellationToken)) {
            this.Editor = new CancellationTokenEditor(this);
        }
        else if (type == typeof(char)) {
            this.Editor = new CharEditor(this);
        }
        else if (type == typeof(byte)) {
            this.Editor = new ByteEditor(this);
        }
        else if (type == typeof(Uri)) {
            this.Editor = new UriEditor(this);
        }

        //--
        else if (type == typeof(int?)) {
            this.Editor = new NullableIntEditor(this);
        }
        else if (type == typeof(short?)) {
            this.Editor = new NullableInt16Editor(this);
        }
        else if (type == typeof(long?)) {
            this.Editor = new NullableInt64Editor(this);
        }
        else if (type == typeof(uint?)) {
            this.Editor = new NullableUIntEditor(this);
        }
        else if (type == typeof(ushort?)) {
            this.Editor = new NullableUInt16Editor(this);
        }
        else if (type == typeof(ulong?)) {
            this.Editor = new NullableUInt64Editor(this);
        }
        else if (type == typeof(decimal?)) {
            this.Editor = new NullableDecimalEditor(this);
        }
        else if (type == typeof(double?)) {
            this.Editor = new NullableDoubleEditor(this);
        }
        else if (type == typeof(float?)) {
            this.Editor = new NullableFloat32Editor(this);
        }
        else if (type == typeof(bool?)) {
            this.Editor = new NullableBoolEditor(this);
        }
        else if (type == typeof(DateTime?)) {
            this.Editor = new NullableDateTimeEditor(this);
        }
        else if (type == typeof(DateTimeOffset?)) {
            this.Editor = new NullableDateTimeOffsetEditor(this);
        }
        else if (type == typeof(TimeSpan?)) {
            this.Editor = new NullableTimeSpanEditor(this);
        }
        else {
            throw new Exception($"Unable to create an editor. Unknown system type {type.FullName}");
        }
    }

    public ITypeEditor Editor { get; }
    public override string FormattedTypeName { get; }

    public override string FormattedValue => this.Editor.FormattedValue;

    public override bool IsEditing {
        get => this.Editor.IsEditing;
        set => this.Editor.IsEditing = value;
    }
 
    public EnvVarNodeViewModel EnvVar { get; }
   
    public override void Dispose() {
        base.Dispose();
        this.Editor.Dispose();
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
        //no child nodes
    }
}