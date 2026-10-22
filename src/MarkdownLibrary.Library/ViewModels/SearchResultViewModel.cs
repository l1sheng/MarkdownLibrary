using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 搜索结果：书籍命中进入详情，笔记命中直接进入编辑。
/// SetParameter 接收关键词字符串。
/// </summary>
public class SearchResultViewModel : ViewModelBase
{
    private readonly ISearchService _searchService;
    private readonly IBookStorage _bookStorage;
    private readonly INoteStorage _noteStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IAlertService _alertService;

    private string _keyword = string.Empty;
    private string _statusText = "正在搜索…";

    public SearchResultViewModel(ISearchService searchService, IBookStorage bookStorage,
        INoteStorage noteStorage, IContentNavigationService contentNavigationService,
        IAlertService alertService)
    {
        _searchService = searchService;
        _bookStorage = bookStorage;
        _noteStorage = noteStorage;
        _contentNavigationService = contentNavigationService;
        _alertService = alertService;

        OpenHitCommand = new AsyncRelayCommand<SearchHit>(OpenHitAsync);

        Results.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(CountText));
        };
    }

    public ObservableCollection<SearchHit> Results { get; } = new();

    public string Keyword
    {
        get => _keyword;
        private set
        {
            if (SetProperty(ref _keyword, value))
            {
                OnPropertyChanged(nameof(PageTitle));
            }
        }
    }

    public bool IsEmpty => Results.Count == 0;

    public string CountText => $"共 {Results.Count} 条结果";

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public override string PageTitle => string.IsNullOrWhiteSpace(Keyword) ? "搜索结果" : $"搜索：{Keyword}";

    public ICommand OpenHitCommand { get; }

    public override void SetParameter(object parameter)
    {
        if (parameter is string keyword)
        {
            Keyword = keyword;
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            StatusText = "正在搜索…";
            var hits = await _searchService.SearchAsync(Keyword);

            Results.Clear();
            foreach (var hit in hits)
            {
                Results.Add(hit);
            }

            StatusText = Results.Count == 0
                ? $"没有找到与“{Keyword}”相关的内容"
                : $"共 {Results.Count} 条结果";
        }
        catch (Exception exception)
        {
            StatusText = "搜索失败";
            await _alertService.ShowAsync("搜索失败", exception.Message);
        }
    }

    private async Task OpenHitAsync(SearchHit hit)
    {
        if (hit is null)
        {
            return;
        }

        var book = await _bookStorage.GetByIdAsync(hit.BookId);
        if (book is null)
        {
            await _alertService.ShowAsync("打开失败", "这条结果对应的书已经不在书库里了。");
            return;
        }

        if (hit.Kind == "note" && hit.NoteId > 0)
        {
            var note = await _noteStorage.GetByIdAsync(hit.NoteId);
            if (note is not null)
            {
                _contentNavigationService.NavigateTo(ContentNavigationConstant.NoteEdit,
                    new NoteEditContext { Book = book, Note = note });
                return;
            }
        }

        _contentNavigationService.NavigateTo(ContentNavigationConstant.BookDetail, book);
    }
}
