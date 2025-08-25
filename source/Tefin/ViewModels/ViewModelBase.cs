#region

using System.Linq.Expressions;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Utils;

#endregion

namespace Tefin.ViewModels;

public class ViewModelBase : ReactiveObject, IDisposable {
    private readonly List<IDisposable> _disposables = new();
    private bool _isBusy;
    public static ICommand EmptyCommand { get; } = ReactiveCommand.Create(() => { });
    public IOs Io { get; } = Resolver.value;

    public bool IsBusy {
        get => this._isBusy;
        set => this.RaiseAndSetIfChanged(ref this._isBusy, value);
    }

    public virtual void Dispose() {
        foreach (var d in this._disposables) {
            d?.Dispose();
        }
    }

    public void SubscribeTo<R>(Expression<Func<ViewModelBase, R>> prop, Action<ViewModelBase> onChanged) {
        var b = (MemberExpression)prop.Body;
        this.Subscribe(b.Member.Name, onChanged).Then(this.MarkForCleanup);
    }

    public void SubscribeTo<R,V>(Expression<Func<V, R>> prop, Action<V> onChanged) where V : ViewModelBase {
        var b = (MemberExpression)prop.Body;
        this.Subscribe(b.Member.Name, vb => onChanged((V)vb)).Then(this.MarkForCleanup);
    }

    protected ICommand CreateCommand(Func<Task> doThis) =>
        ReactiveCommand.Create(async () => {
            try {
                await doThis();
            }
            catch (Exception exc) {
                this.Io.Log.Error(exc);
            }
        });

    
    protected ICommand CreateCommand<T>(Func<T, Task> doThis) =>
        ReactiveCommand.CreateFromTask<T>(async (arg) => {
            try {
                this.IsBusy = true;
                await doThis(arg);
            }
            catch (Exception exc) {
                this.Io.Log.Error(exc);
            }
            finally {
                this.IsBusy = false;
            }
        });

    protected ICommand CreateCommand(Action doThis) =>
        ReactiveCommand.Create(() => {
            try {
                doThis();
            }
            catch (Exception exc) {
                this.Io.Log.Error(exc);
            }
        });

    protected ICommand CreateCommand<T>(Action<T> doThis) =>
        ReactiveCommand.Create<T>((arg) => {
            try {
                doThis(arg);
            }
            catch (Exception exc) {
                this.Io.Log.Error(exc);
            }
        });

    protected void MarkForCleanup(IDisposable d) => this._disposables.Add(d);

    protected void Exec(Action a) {
        try {
            a();
        }
        catch (Exception e) {
            this.Io.Log.Error(e);
        }
    }

    protected async Task Exec(Func<Task> a) {
        try {
            await a();
        }
        catch (Exception e) {
            this.Io.Log.Error(e);
        }
    }
}