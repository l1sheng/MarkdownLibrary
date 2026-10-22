using System.Collections.Generic;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Models;
using Microsoft.Data.Sqlite;

namespace MarkdownLibrary.Library.Services;

public interface IMarkdownService
{
    /// <summary>读取并解析 Markdown 文件：front matter、目录、纯文本、字数。</summary>
    Task<BookDocument> LoadAsync(string filePath);

    Task<string> ReadRawAsync(string filePath);
}

public interface ILibraryScanService
{
    /// <summary>扫描目录，把新增的 .md 文件登记进书库。</summary>
    Task<ScanResult> ScanAsync(string folder);
}

public interface ISearchService
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(string keyword);
}

public interface IStatisticsService
{
    Task<StatisticsOverview> GetOverviewAsync();
}

public interface IPreferenceStorage
{
    string Get(string key, string defaultValue = null);

    void Set(string key, string value);
}

public interface IAlertService
{
    Task ShowAsync(string title, string message);

    Task<bool> ConfirmAsync(string title, string message);
}

public interface IFileDialogService
{
    Task<string> PickFolderAsync();

    Task<string> PickMarkdownFileAsync();
}

public interface IDatabase
{
    void Initialize();

    SqliteConnection CreateConnection();
}
