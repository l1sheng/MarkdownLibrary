namespace MarkdownLibrary.Library.Models;

/// <summary>
/// 标签，对应 tags 表；与书籍是多对多关系。
/// </summary>
public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
