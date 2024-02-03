#region

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;

#endregion

namespace Tefin.ViewModels.Types;

public class NodeTemplateColumn<T> : TemplateColumn<T> {
    public NodeTemplateColumn(object? header, IDataTemplate cellTemplate, IDataTemplate? cellEditingTemplate = null,
        GridLength? width = null,
        TemplateColumnOptions<T>? options = null) : base(header, cellTemplate, cellEditingTemplate, width, options) {
    }

    public NodeTemplateColumn(object? header, object cellTemplateResourceKey,
        object? cellEditingTemplateResourceKey = null, GridLength? width = null,
        TemplateColumnOptions<T>? options = null) : base(header, cellTemplateResourceKey,
        cellEditingTemplateResourceKey, width, options) {
    }

    public override ICell CreateCell(IRow<T> row) {
        if (row.Model is MetadataEntryNode)
            //these nodes will not show the CellEditingTemplate i.e. set to null = not editable
        {
            return new TemplateCell(row.Model, this.GetCellTemplate, null, this.Options);
        }

        //editable nodes
        return base.CreateCell(row);
    }
}