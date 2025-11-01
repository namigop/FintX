using System.Reactive;
using System.Windows.Input;

namespace Tefin.ViewModels.Overlay;

public class YesNoOverlayViewModel : DialogViewModel {
    public YesNoOverlayViewModel(string title, string message, ICommand yesCommand, ICommand noCommand) : base(title,
        DialogType.Question) {
        this.Message = message;

        this.YesCommand = this.CreateCommand(() => {
            yesCommand.Execute(Unit.Default);
            this.Close();
        });

        this.NoCommand = this.CreateCommand(() => {
            noCommand.Execute(Unit.Default);
            this.Close();
        });
    }

    public string Message { get; }

    public ICommand NoCommand { get; }

    public ICommand YesCommand { get; }
}