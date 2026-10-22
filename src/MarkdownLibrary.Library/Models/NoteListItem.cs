using System;

namespace MarkdownLibrary.Library.Models;

/// <summary>
/// 笔记列表项：在 Note 上补充书名，便于“全部笔记”页展示。
/// </summary>
public class NoteListItem : Note
{
    public string BookTitle { get; set; } = string.Empty;

    public string Title
    {
        get
        {
            var source = string.IsNullOrWhiteSpace(Content) ? Excerpt : Content;
            if (string.IsNullOrWhiteSpace(source))
            {
                return "（空笔记）";
            }

            var line = source.Replace("\r", " ").Replace("\n", " ").Trim();
            return line.Length <= 28 ? line : line[..28] + "…";
        }
    }

    public string Snippet
    {
        get
        {
            var line = string.IsNullOrWhiteSpace(Content) ? Excerpt : Content;
            if (string.IsNullOrWhiteSpace(line))
            {
                return "还没有写内容";
            }

            line = line.Replace("\r", " ").Replace("\n", " ").Trim();
            return line.Length <= 90 ? line : line[..90] + "…";
        }
    }

    public string UpdatedText => UpdatedAt.ToString("yyyy-MM-dd HH:mm");
}
