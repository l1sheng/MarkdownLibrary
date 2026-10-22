using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

/// <summary>
/// 扫描书库目录：把新增的 .md 文件登记进数据库，已存在的更新元数据。
/// </summary>
public class LibraryScanService : ILibraryScanService
{
    private readonly IBookStorage _bookStorage;
    private readonly IMarkdownService _markdownService;

    public LibraryScanService(IBookStorage bookStorage, IMarkdownService markdownService)
    {
        _bookStorage = bookStorage;
        _markdownService = markdownService;
    }

    public async Task<ScanResult> ScanAsync(string folder)
    {
        var result = new ScanResult { Folder = folder };

        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return result;
        }

        var files = Directory.EnumerateFiles(folder, "*.md", SearchOption.AllDirectories)
            .Where(IsNotIgnored)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var file in files)
        {
            try
            {
                var document = await _markdownService.LoadAsync(file);
                var existing = await _bookStorage.GetByPathAsync(file);

                if (existing is null)
                {
                    await _bookStorage.AddAsync(new Book
                    {
                        Title = document.Metadata.Title,
                        Author = document.Metadata.Author,
                        FilePath = file,
                        CoverPath = ResolveCover(file, document.Metadata.Cover),
                        WordCount = document.WordCount,
                        Status = BookStatus.Unread,
                        CreatedAt = DateTime.Now
                    });
                    result.Added++;
                }
                else
                {
                    existing.Title = document.Metadata.Title;
                    existing.Author = document.Metadata.Author;
                    existing.WordCount = document.WordCount;
                    existing.CoverPath = ResolveCover(file, document.Metadata.Cover);
                    await _bookStorage.UpdateAsync(existing);
                    result.Updated++;
                }
            }
            catch
            {
                // 单个文件解析失败不影响整个书库扫描。
                result.Skipped++;
            }
        }

        return result;
    }

    private static bool IsNotIgnored(string path)
    {
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return !parts.Any(part => part is ".git" or "obj" or "bin" or "node_modules" or ".vs");
    }

    private static string ResolveCover(string filePath, string cover)
    {
        if (string.IsNullOrWhiteSpace(cover))
        {
            return string.Empty;
        }

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        return Path.GetFullPath(Path.Combine(directory, cover));
    }
}
