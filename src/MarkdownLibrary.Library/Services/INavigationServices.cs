namespace MarkdownLibrary.Library.Services;

public static class RootNavigationConstant
{
    public const string Initialization = "Initialization";
    public const string Main = "Main";
}

public static class MenuNavigationConstant
{
    public const string Shelf = "Shelf";
    public const string Notes = "Notes";
    public const string Search = "Search";
    public const string Stats = "Stats";
    public const string Settings = "Settings";
}

public static class ContentNavigationConstant
{
    public const string BookDetail = "BookDetail";
    public const string Reader = "Reader";
    public const string NoteEdit = "NoteEdit";
    public const string SearchResult = "SearchResult";
}

/// <summary>
/// 根导航：在初始化页与主界面之间切换。
/// </summary>
public interface IRootNavigationService
{
    void NavigateTo(string pageKey);
}

/// <summary>
/// 菜单导航：切换主内容区的首页。
/// </summary>
public interface IMenuNavigationService
{
    void NavigateTo(string pageKey);
}

/// <summary>
/// 内容导航：把详情页、阅读页压入返回栈。
/// </summary>
public interface IContentNavigationService
{
    void NavigateTo(string pageKey, object parameter = null);

    void GoBack();
}
