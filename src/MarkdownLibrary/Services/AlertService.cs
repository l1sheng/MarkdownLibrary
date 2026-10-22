using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Services;

/// <summary>
/// 统一弹窗：把“通知用户”从 ViewModel 中剥离。
/// </summary>
public class AlertService : IAlertService
{
    public async Task ShowAsync(string title, string message)
    {
        var okButton = new Button
        {
            Content = "确定",
            MinWidth = 88,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 18
        };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        });
        panel.Children.Add(okButton);

        var window = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = panel
        };

        okButton.Click += (_, _) => window.Close();
        await ShowWindowAsync(window);
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var completion = new TaskCompletionSource<bool>();

        var okButton = new Button { Content = "确定", MinWidth = 88 };
        var cancelButton = new Button { Content = "取消", MinWidth = 88 };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        buttons.Children.Add(cancelButton);
        buttons.Children.Add(okButton);

        var panel = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 18
        };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        });
        panel.Children.Add(buttons);

        var window = new Window
        {
            Title = title,
            Width = 440,
            Height = 210,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = panel
        };

        okButton.Click += (_, _) =>
        {
            completion.TrySetResult(true);
            window.Close();
        };
        cancelButton.Click += (_, _) =>
        {
            completion.TrySetResult(false);
            window.Close();
        };
        window.Closed += (_, _) => completion.TrySetResult(false);

        await ShowWindowAsync(window);
        return await completion.Task;
    }

    private static async Task ShowWindowAsync(Window window)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            { MainWindow: { } owner } && owner.IsVisible)
        {
            await window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }
}
