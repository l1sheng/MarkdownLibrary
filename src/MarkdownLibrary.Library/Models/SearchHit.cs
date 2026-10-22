using System;
using System.Collections.Generic;

namespace MarkdownLibrary.Library.Models;

/// <summary>
/// 搜索结果：可能命中书名，也可能命中笔记。
/// </summary>
public class SearchHit
{
    public string Kind { get; set; } = "book";

    public int BookId { get; set; }

    public int NoteId { get; set; }

    public string BookTitle { get; set; } = string.Empty;

    public string Snippet { get; set; } = string.Empty;

    public string KindText => Kind == "note" ? "笔记" : "书籍";
}

/// <summary>
/// 扫描书库的结果。
/// </summary>
public class ScanResult
{
    public string Folder { get; set; } = string.Empty;

    public int Added { get; set; }

    public int Updated { get; set; }

    public int Skipped { get; set; }

    public int Total => Added + Updated + Skipped;
}

public class DailyMinutes
{
    public DateTime Date { get; set; }

    public int Minutes { get; set; }

    public string DayText => Date.ToString("MM-dd");

    public string MinutesText => $"{Minutes} 分钟";

    public double BarHeight => Math.Max(2, Math.Min(120, Minutes * 2.0));
}

/// <summary>
/// 统计页需要的聚合数据。
/// </summary>
public class StatisticsOverview
{
    public int BookCount { get; set; }

    public int NoteCount { get; set; }

    public int TotalMinutes { get; set; }

    public int TodayMinutes { get; set; }

    public int StreakDays { get; set; }

    public IReadOnlyList<DailyMinutes> LastSevenDays { get; set; } = new List<DailyMinutes>();

    public string TotalHoursText => $"{TotalMinutes / 60.0:0.0} 小时";
}
