#region

using System.ComponentModel;

using Tefin.ViewModels;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.Utils;

public static class PropChange {
    // public static IDisposable Subscribe(this INotifyPropertyChanged obj, string propName,
    //     Action<INotifyPropertyChanged> onChanged) {
    //     return new Sub(obj, propName, onChanged);
    // }

    public static IDisposable Subscribe(this IExplorerItem obj, string propName, Action<IExplorerItem> onChanged) =>
        Subscribe((NodeBase)obj, propName, i => onChanged(i));

    //return new Sub((NodeBase)obj, propName, i => onChanged((NodeBase)i));
    public static IDisposable Subscribe<T>(this T obj, string propName, Action<T> onChanged) where T : ViewModelBase =>
        new Sub<T>(obj, propName, i => onChanged(i));

    public class Sub<T> : IDisposable where T : INotifyPropertyChanged {
        private readonly T _obj;
        private readonly Action<T> _onChanged;
        private readonly string _propName;

        public Sub(T obj, string propName, Action<T> onChanged) {
            this._obj = obj;
            this._propName = propName;
            this._onChanged = onChanged;
            obj.PropertyChanged += this.OnPropChanged;
        }

        public void Dispose() => this._obj.PropertyChanged -= this.OnPropChanged;

        private void OnPropChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == this._propName) {
                this._onChanged(this._obj);
            }
        }
    }
}