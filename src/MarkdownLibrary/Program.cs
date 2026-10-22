using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using MarkdownLibrary.Library.Helpers;

namespace MarkdownLibrary;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        LogStartup($"start args={string.Join(' ', args)}");

        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log(e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log(e.Exception);
            e.SetObserved();
        };

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    public static void LogStartup(string message)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "mdlib-start.log");
            File.AppendAllText(path, $"[{DateTime.Now:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // 忽略日志失败。
        }
    }

    private static void Log(Exception exception)
    {
        if (exception is null)
        {
            return;
        }

        LogStartup("fatal: " + exception);

        try
        {
            var path = Path.Combine(PathHelper.DataDirectory, "error.log");
            File.AppendAllText(path, $"[{DateTime.Now:O}] {exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // 日志写不进去时不再抛出，避免掩盖原始异常。
        }
    }
}
