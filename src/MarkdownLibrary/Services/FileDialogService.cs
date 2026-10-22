using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Services;

public class FileDialogService : IFileDialogService
{
    public async Task<string> PickFolderAsync()
    {
        var topLevel = GetTopLevel();
        if (topLevel is null)
        {
            return null;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择 Markdown 书库目录",
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    public async Task<string> PickMarkdownFileAsync()
    {
        var topLevel = GetTopLevel();
        if (topLevel is null)
        {
            return null;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择 Markdown 文件",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Markdown")
                {
                    Patterns = new[] { "*.md", "*.markdown" }
                }
            }
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    private static TopLevel GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            { MainWindow: { } window })
        {
            return TopLevel.GetTopLevel(window);
        }

        return null;
    }
}
