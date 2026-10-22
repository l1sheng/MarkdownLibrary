using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Helpers;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Views;

namespace MarkdownLibrary;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var locator = new ServiceLocator();
                locator.Initialize();
                Program.LogStartup("service locator ready");

                desktop.MainWindow = new MainWindow
                {
                    DataContext = locator.MainWindowViewModel
                };

                locator.RootNavigationService.NavigateTo(RootNavigationConstant.Main);
                locator.MenuNavigationService.NavigateTo(MenuNavigationConstant.Shelf);
                Program.LogStartup("main window ready");

                var arguments = Environment.GetCommandLineArgs();

                if (arguments.Contains("--smoke-deep"))
                {
                    RunDeepSmoke(locator, desktop);
                }
                else if (arguments.Contains("--smoke"))
                {
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(4)
                    };
                    timer.Tick += (_, _) =>
                    {
                        timer.Stop();
                        Program.LogStartup("smoke done");
                        desktop.Shutdown();
                    };
                    timer.Start();
                    Program.LogStartup("smoke timer started");
                }
            }
        }
        catch (Exception exception)
        {
            Program.LogStartup("startup failed: " + exception);
            throw;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 开发期冒烟：自动走一遍全部页面并截图，验证 ViewLocator、导航与 Markdown 控件。
    /// </summary>
    private async void RunDeepSmoke(ServiceLocator locator,
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            await Task.Delay(800);
            Capture(desktop, "shelf.png");

            var books = await locator.BookStorage.GetAllAsync();
            var book = books.FirstOrDefault(x => x.Title.Contains("论语")) ?? books.FirstOrDefault();

            if (book is not null)
            {
                locator.BookDetailViewModel.SetParameter(book);
                locator.MainViewModel.PushContent(locator.BookDetailViewModel);
                await Task.Delay(700);

                locator.BookDetailViewModel.NewTagName = "经典";
                if (locator.BookDetailViewModel.AddTagCommand is IAsyncRelayCommand addTagCommand)
                {
                    await addTagCommand.ExecuteAsync(null);
                }

                await Task.Delay(300);
                Capture(desktop, "detail.png");

                locator.ReaderViewModel.SetParameter(book);
                locator.MainViewModel.PushContent(locator.ReaderViewModel);
                await Task.Delay(1800);
                Capture(desktop, "reader.png");
                Program.LogStartup("deep smoke: detail/reader");
            }

            locator.MenuNavigationService.NavigateTo(MenuNavigationConstant.Notes);
            await Task.Delay(700);
            Capture(desktop, "notes.png");
            Program.LogStartup("deep smoke: notes");

            if (book is not null)
            {
                locator.NoteEditViewModel.SetParameter(new NoteEditContext { Book = book });
                locator.MainViewModel.PushContent(locator.NoteEditViewModel);
                await Task.Delay(600);
                Capture(desktop, "note-edit.png");
                locator.MainViewModel.GoBack();
                Program.LogStartup("deep smoke: note edit");
            }

            locator.MenuNavigationService.NavigateTo(MenuNavigationConstant.Search);
            await Task.Delay(500);
            Capture(desktop, "search.png");

            locator.SearchResultViewModel.SetParameter("道");
            locator.MainViewModel.PushContent(locator.SearchResultViewModel);
            await Task.Delay(700);
            Capture(desktop, "search-result.png");
            locator.MainViewModel.GoBack();
            Program.LogStartup("deep smoke: search");

            locator.MenuNavigationService.NavigateTo(MenuNavigationConstant.Stats);
            await Task.Delay(800);
            Capture(desktop, "stats.png");
            Program.LogStartup("deep smoke: stats");

            Program.LogStartup("deep smoke: ok");
        }
        catch (Exception exception)
        {
            Program.LogStartup("deep smoke failed: " + exception);
        }
        finally
        {
            desktop.Shutdown();
        }
    }

    private static void Capture(IClassicDesktopStyleApplicationLifetime desktop, string fileName)
    {
        try
        {
            var window = desktop.MainWindow;
            if (window is null || window.Bounds.Width < 1 || window.Bounds.Height < 1)
            {
                Program.LogStartup($"capture {fileName}: window not ready");
                return;
            }

            var size = new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height);
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96));
            bitmap.Render(window);

            var path = Path.Combine(PathHelper.DataDirectory, fileName);
            bitmap.Save(path);
            Program.LogStartup($"capture {fileName}: saved");
        }
        catch (Exception exception)
        {
            Program.LogStartup($"capture {fileName} failed: {exception.Message}");
        }
    }
}
