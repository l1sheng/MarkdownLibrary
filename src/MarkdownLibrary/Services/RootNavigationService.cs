using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Services;

public class RootNavigationService : IRootNavigationService
{
    public void NavigateTo(string pageKey)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (desktop.MainWindow?.DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.Content = pageKey switch
        {
            RootNavigationConstant.Main => viewModel.Main,
            _ => viewModel.Main
        };
    }
}
