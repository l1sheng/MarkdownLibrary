using System;

namespace MarkdownLibrary.Library.Models;

public enum BookStatus
{
    Unread = 0,
    Reading = 1,
    Finished = 2
}

/// <summary>
/// 书库中的一本书，对应 books 表。
/// </summary>
public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string CoverPath { get; set; } = string.Empty;

    public int WordCount { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Unread;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastOpenedAt { get; set; }

    public string StatusText => Status switch
    {
        BookStatus.Reading => "在读",
        BookStatus.Finished => "已读完",
        _ => "未读"
    };

    public string WordCountText => WordCount >= 10000
        ? $"{WordCount / 10000.0:0.0} 万字"
        : $"{WordCount} 字";

    public string SubtitleText
    {
        get
        {
            var author = string.IsNullOrWhiteSpace(Author) ? "佚名" : Author;
            return $"{author} · {WordCountText} · {StatusText}";
        }
    }
}
