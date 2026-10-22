using System.Collections.Generic;

namespace MarkdownLibrary.Library.Models;

/// <summary>
/// Markdown 文件的 front matter 元数据。
/// </summary>
public class BookMetadata
{
    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Cover { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// 目录中的一项，由 Markdown 标题生成。
/// </summary>
public class TocItem
{
    public int Index { get; set; }

    public int Level { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Indent => new string(' ', (Level - 1) * 2);
}

/// <summary>
/// 解析后的书籍文档。
/// </summary>
public class BookDocument
{
    public BookMetadata Metadata { get; set; } = new();

    public string Markdown { get; set; } = string.Empty;

    public string PlainText { get; set; } = string.Empty;

    public int WordCount { get; set; }

    public List<TocItem> Toc { get; set; } = new();
}
