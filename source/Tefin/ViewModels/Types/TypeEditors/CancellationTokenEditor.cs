#region

using System.Threading;

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class CancellationTokenEditor : TypeEditorBase<CancellationToken> {
    private bool _isNone;

    public CancellationTokenEditor(TypeBaseNode node) : base(node) {
        this.SubscribeTo(x => ((CancellationTokenEditor)x).IsNone, this.OnIsNoneChanged);
        var token = (CancellationToken)((CancellationTokenNode)this.Node).Value;
        this._isNone = token == CancellationToken.None;
    }

    public bool IsNone {
        get => this._isNone;
        set => this.RaiseAndSetIfChanged(ref this._isNone, value);
    }

    private void OnIsNoneChanged(ViewModelBase obj) {
        if (((CancellationTokenEditor)obj).IsNone) {
            this.TempValue = CancellationToken.None;
        }
        else {
            var c = (CancellationTokenNode)this.Node;
            c.Source = new CancellationTokenSource();
            c.Value = c.Source.Token;
        }
    }
}