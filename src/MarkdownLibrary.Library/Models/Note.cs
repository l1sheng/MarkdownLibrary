using System;

namespace MarkdownLibrary.Library.Models;

/// <summary>
/// 一条摘录笔记，对应 notes 表。
/// </summary>
public class Note
{
    public int Id { get; set; }

    public int BookId { get; set; }

    /// <summary>原文摘录。</summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>自己的想法。</summary>
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string DisplayText => string.IsNullOrWhiteSpace(Content) ? Excerpt : Content;
}
