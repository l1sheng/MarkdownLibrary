using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 搜索入口：输入关键词后跳到结果页。
/// </summary>
public class SearchViewModel : ViewModelBase
{
    private readonly IContentNavigationService _contentNavigationService;

    private string _keyword = string.Empty;

    public SearchViewModel(IContentNavigationService contentNavigationService)
    {
        _contentNavigationService = contentNavigationService;
        SearchCommand = new RelayCommand(Search);
    }

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
    }

    public override string PageTitle => "搜索";

    public string Hint => "支持书名、作者与笔记内容；输入关键词后按回车或点“搜索”。";

    public ICommand SearchCommand { get; }

    private void Search()
    {
        if (string.IsNullOrWhiteSpace(Keyword))
        {
            return;
        }

        _contentNavigationService.NavigateTo(ContentNavigationConstant.SearchResult, Keyword.Trim());
    }
}
