using System;

namespace MarkdownLibrary.Library.Models;

/// <summary>
/// 阅读进度：标题索引 + 章内比例，窗口重排后依然可靠。
/// </summary>
public class ReadingProgress
{
    public int BookId { get; set; }

    public int HeadingIndex { get; set; }

    public double OffsetRatio { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 一次阅读会话，用于统计阅读时长与连续天数。
/// </summary>
public class ReadingSession
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime EndedAt { get; set; }

    public int Minutes { get; set; }
}
