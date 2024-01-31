#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

#endregion

namespace Tefin.Utils;

public static class DialogUtils {

    public static Window GetMainWindow() {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            return desktop.MainWindow!;
        }

        throw new NotSupportedException();
    }

    public static async Task<(bool, string[])> OpenFile(string dialogTitle, string fileTitle, string[] filterExtensions, bool allowMultipleSelection = false) {
        var topLevel = TopLevel.GetTopLevel(GetMainWindow());
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = dialogTitle,
            AllowMultiple = allowMultipleSelection,
            FileTypeFilter = new[] {
                new FilePickerFileType(fileTitle) {
                    Patterns = filterExtensions
                }
            }
        });

        if (files.Count >= 1) {
            var filePaths = files.Where(t => t.Path.IsFile).Select(t => t.Path.LocalPath).ToArray();
            return (true, filePaths);
        }

        return (false, Array.Empty<string>());
    }
    public static async Task<string> SelectFile(string dialogTitle, string fileName,  string fileTitle, string extension) {
        var topLevel = TopLevel.GetTopLevel(GetMainWindow());

        // Start async operation to open the dialog.
        var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = dialogTitle,
            ShowOverwritePrompt = true,
            SuggestedFileName = fileName,
            FileTypeChoices = new[] {
                new FilePickerFileType(fileTitle) {
                    Patterns = new[] {
                        extension
                    }
                }
            }
        });

        return file?.Path.LocalPath ?? string.Empty;
    }

    public static async Task SaveFile(string dialogTitle, string fileName, string content, string fileTitle, string extension) {
        var topLevel = TopLevel.GetTopLevel(GetMainWindow());

        // Start async operation to open the dialog.
        var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = dialogTitle,
            ShowOverwritePrompt = true,
            SuggestedFileName = fileName,
            FileTypeChoices = new[] {
                new FilePickerFileType(fileTitle) {
                    Patterns = new[] {
                        extension
                    }
                }
            }
        });

        if (file is not null) {
            await using var stream = await file.OpenWriteAsync();
            await using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(content);
        }
    }

    public static async Task<string> SelectFolder() {
        var topLevel = TopLevel.GetTopLevel(GetMainWindow());
        var folders = await topLevel!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() {
            Title = "Select a project folder",
            AllowMultiple = false,
        });

        return folders.Any() ? folders.First().Path.LocalPath : "";
    }
}