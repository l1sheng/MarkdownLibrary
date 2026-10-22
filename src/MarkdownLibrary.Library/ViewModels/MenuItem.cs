using System.Collections.Generic;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 侧边菜单项。
/// </summary>
public class MenuItem
{
    public string View { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Icon { get; init; } = string.Empty;

    public static IReadOnlyList<MenuItem> MenuItems { get; } = new List<MenuItem>
    {
        new() { View = MenuNavigationConstant.Shelf, Name = "书架", Icon = "📚" },
        new() { View = MenuNavigationConstant.Notes, Name = "笔记", Icon = "📝" },
        new() { View = MenuNavigationConstant.Search, Name = "搜索", Icon = "🔍" },
        new() { View = MenuNavigationConstant.Stats, Name = "统计", Icon = "📊" },
        new() { View = MenuNavigationConstant.Settings, Name = "设置", Icon = "⚙" }
    };
}
