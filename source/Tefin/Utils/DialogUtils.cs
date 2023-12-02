using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Tefin.Utils;

public static class DialogUtils {
    public static async Task<(bool, string[])> OpenFile(FileDialogFilter filter, bool allowMultipleSelection = false) {
        var dg = new OpenFileDialog();
       
        dg.Filters = new List<FileDialogFilter>() { filter }; //"txt files (*.txt)|*.txt|All files (*.*)|*.*";
        dg.AllowMultiple = allowMultipleSelection;
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var files = await dg.ShowAsync(desktop.MainWindow);
            if (files == null || files.Length == 0) {
                return (false, Array.Empty<string>());
            }

            return (true, files);
        }

        return (false, Array.Empty<string>());
    }
}