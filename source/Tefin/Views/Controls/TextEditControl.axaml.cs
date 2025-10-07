#region

using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI;

#endregion

namespace Tefin.Views.Controls;

public class TextEditControl : TemplatedControl {

    public TextEditControl() {
        this._canEdit = true;
        
        this._commitEditCommand = ReactiveCommand.Create(() => {
            this.TextToEdit = this.TempTextToEdit;
            //this.SetValue(TextToEditProperty, this.TempTextToEdit);
            //Dispatcher.UIThread.Invoke(() => {
            //    this.SetValue(TextToEditProperty, this.TempTextToEdit);
                //this.TextToEdit = this.TempTextToEdit;
            //    this.CanEdit = true;
            //    RaisePropertyChanged(CanStopEditProperty, true, false);
            //});
        });
        
        this._editCommand = ReactiveCommand.Create(() => {
            Dispatcher.UIThread.Invoke(() => {
                //this.SetValue(TempTextToEditProperty, this.TextToEdit);
                this.TempTextToEdit = this.TextToEdit;
                this.CanEdit = false;
                RaisePropertyChanged(CanStopEditProperty, false, true);
            });
            });
            
    }
    public static readonly StyledProperty<string> TextToEditProperty =
        AvaloniaProperty.Register<TextEditControl, string>(nameof(TextToEdit), "", defaultBindingMode: BindingMode.TwoWay);

    public string TextToEdit {
        get => this.GetValue(TextToEditProperty);
        set => this.SetValue(TextToEditProperty, value);
    }
    
    public static readonly StyledProperty<string> TempTextToEditProperty =
        AvaloniaProperty.Register<TextEditControl, string>(nameof(TempTextToEdit), "", defaultBindingMode: BindingMode.TwoWay);

    public string TempTextToEdit {
        get => this.GetValue(TempTextToEditProperty);
        set => this.SetValue(TempTextToEditProperty, value);
    }
    
    // public static readonly StyledProperty<bool> CanEditProperty =
    //     AvaloniaProperty.Register<TextEditControl, bool>(nameof(CanEdit), true, defaultBindingMode: BindingMode.TwoWay);
    //
    // public static readonly StyledProperty<bool> CanStopEditProperty =
    //     AvaloniaProperty.Register<TextEditControl, bool>(nameof(CanEdit), false, defaultBindingMode: BindingMode.TwoWay);

    
    
    
    public static readonly DirectProperty<TextEditControl, ICommand> CommitEditCommandProperty =
        AvaloniaProperty.RegisterDirect<TextEditControl, ICommand>(
            nameof(CommitEditCommand),
            o => o.CommitEditCommand);

    private ICommand _commitEditCommand;
    public ICommand  CommitEditCommand {
        get => this._commitEditCommand;
        set => this.SetAndRaise(CommitEditCommandProperty, ref _commitEditCommand, value);
    }
    
    
    public static readonly DirectProperty<TextEditControl, ICommand> EditCommandProperty =
        AvaloniaProperty.RegisterDirect<TextEditControl, ICommand>(
            nameof(EditCommand),
            o => o.EditCommand);

    private ICommand _editCommand;
    public ICommand  EditCommand {
        get => this._editCommand;
        set => this.SetAndRaise(EditCommandProperty, ref _editCommand, value);
    }
    
    
    public static readonly DirectProperty<TextEditControl, bool> CanEditProperty =
        AvaloniaProperty.RegisterDirect<TextEditControl, bool>(
            nameof(CanEdit),
            o => o.CanEdit);

    private bool _canEdit;
    public bool  CanEdit {
        get => this._canEdit;
        set => this.SetAndRaise(CanEditProperty, ref _canEdit, value);
    }
    
    public static readonly DirectProperty<TextEditControl, bool> CanStopEditProperty =
        AvaloniaProperty.RegisterDirect<TextEditControl, bool>(
            nameof(CanStopEdit),
            o => o.CanStopEdit);
 
    public bool  CanStopEdit {
        get => !this._canEdit;
        set => this.CanEdit = !value;
    }

    
    // public bool CanEdit {
    //     get => this.GetValue(CanEditProperty);
    //     set => this.SetValue(CanEditProperty, value);
    // }
    //
    // public bool CanStopEdit {
    //      get => !this.CanEdit;
    //      set => this.CanEdit = !value;
    // }

    
    // public static readonly StyledProperty<double> WidthProperty = AvaloniaProperty.Register<IconControl, double>(nameof(Width), 24, defaultBindingMode: BindingMode.TwoWay);
    // public double Width {
    //     get => this.GetValue(WidthProperty);
    //     set => this.SetValue(WidthProperty, value);
    // }
}