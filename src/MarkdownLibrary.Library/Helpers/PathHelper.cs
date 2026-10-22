using System;
using System.IO;

namespace MarkdownLibrary.Library.Helpers;

/// <summary>
/// 统一管理数据目录与默认书库目录。
/// 支持用环境变量覆盖，方便开发调试与单元测试。
/// 当系统 AppData 目录因受控文件夹访问等安全策略不可写时，
/// 自动回退到应用程序所在目录下的 .data 子目录。
/// </summary>
public static class PathHelper
{
    private static readonly Lazy<string> _dataDirectory = new(() =>
    {
        var custom = Environment.GetEnvironmentVariable("MDLIB_DATA_DIR");
        if (!string.IsNullOrWhiteSpace(custom))
        {
            Directory.CreateDirectory(custom);
            return custom;
        }

        var primary = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarkdownLibrary");

        try
        {
            Directory.CreateDirectory(primary);
            return primary;
        }
        catch (UnauthorizedAccessException)
        {
            var fallback = Path.Combine(AppContext.BaseDirectory, ".data");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    });

    public static string DataDirectory => _dataDirectory.Value;

    public static string DefaultLibraryDirectory
    {
        get
        {
            var custom = Environment.GetEnvironmentVariable("MDLIB_LIBRARY_DIR");
            if (!string.IsNullOrWhiteSpace(custom))
            {
                return custom;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MarkdownLibrary");
        }
    }

    public static string DatabasePath => Path.Combine(DataDirectory, "library.db");

    public static string PreferencePath => Path.Combine(DataDirectory, "preferences.json");
}