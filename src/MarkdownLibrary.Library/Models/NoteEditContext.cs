namespace MarkdownLibrary.Library.Models;

/// <summary>
/// 打开笔记编辑页时传入的参数：Book 必填，Note 为空表示新建。
/// </summary>
public class NoteEditContext
{
    public Book Book { get; set; }

    public Note Note { get; set; }
}
