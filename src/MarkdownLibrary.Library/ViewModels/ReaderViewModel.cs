using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 阅读器：渲染 Markdown、目录跳转、字号调整、阅读进度保存。
/// </summary>
public class ReaderViewModel : ViewModelBase
{
    private readonly IMarkdownService _markdownService;
    private readonly IReadingProgressStorage _progressStorage;
    private readonly IReadingSessionStorage _sessionStorage;
    private readonly IBookStorage _bookStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IAlertService _alertService;

    private Book _book;
    private string _markdownText = string.Empty;
    private TocItem _selectedTocItem;
    private double _scrollRatio;
    private string _statusText = "正在打开…";
    private DateTime _sessionStart;
    private int _restoreHeadingIndex;
    private double _restoreOffsetRatio;

    public ReaderViewModel(IMarkdownService markdownService,
        IReadingProgressStorage progressStorage, IReadingSessionStorage sessionStorage,
        IBookStorage bookStorage, IContentNavigationService contentNavigationService,
        IAlertService alertService)
    {
        _markdownService = markdownService;
        _progressStorage = progressStorage;
        _sessionStorage = sessionStorage;
        _bookStorage = bookStorage;
        _contentNavigationService = contentNavigationService;
        _alertService = alertService;

        GoBackCommand = new AsyncRelayCommand(GoBackAsync);
    }

    /// <summary>通知 View 把滚动位置恢复到指定标题与章内比例。</summary>
    public event Action<int, double> RestoreScrollRequested;

    /// <summary>通知 View 跳转到目录项。</summary>
    public event Action<int> ScrollToHeadingRequested;

    public ObservableCollection<TocItem> Toc { get; } = new();

    public Book Book
    {
        get => _book;
        private set
        {
            if (SetProperty(ref _book, value))
            {
                OnPropertyChanged(nameof(BookTitle));
            }
        }
    }

    public string BookTitle => Book?.Title ?? "阅读";

    public override string PageTitle => BookTitle;

    public string MarkdownText
    {
        get => _markdownText;
        private set => SetProperty(ref _markdownText, value);
    }

    public TocItem SelectedTocItem
    {
        get => _selectedTocItem;
        set
        {
            if (SetProperty(ref _selectedTocItem, value) && value is not null)
            {
                ScrollToHeadingRequested?.Invoke(value.Index);
            }
        }
    }

    public double ScrollRatio
    {
        get => _scrollRatio;
        set => SetProperty(ref _scrollRatio, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool DocumentReady { get; private set; }

    public int RestoreHeadingIndex => _restoreHeadingIndex;

    public double RestoreOffsetRatio => _restoreOffsetRatio;

    public ICommand GoBackCommand { get; }

    public override void SetParameter(object parameter)
    {
        if (parameter is Book book)
        {
            Book = book;
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            DocumentReady = false;
            StatusText = "正在打开…";
            var document = await _markdownService.LoadAsync(Book.FilePath);

            MarkdownText = document.Markdown;
            Toc.Clear();
            foreach (var item in document.Toc)
            {
                Toc.Add(item);
            }

            var progress = await _progressStorage.GetAsync(Book.Id);
            _restoreHeadingIndex = progress?.HeadingIndex ?? 0;
            _restoreOffsetRatio = progress?.OffsetRatio ?? 0;

            _sessionStart = DateTime.Now;
            await _bookStorage.UpdateLastOpenedAsync(Book.Id, DateTime.Now);

            if (Book.Status == BookStatus.Unread)
            {
                Book.Status = BookStatus.Reading;
                await _bookStorage.UpdateStatusAsync(Book.Id, BookStatus.Reading);
            }

            StatusText = $"{document.WordCount} 字 · {document.Toc.Count} 节";
            DocumentReady = true;
            RestoreScrollRequested?.Invoke(_restoreHeadingIndex, _restoreOffsetRatio);
        }
        catch (Exception ex)
        {
            StatusText = "打开失败";
            await _alertService.ShowAsync("打开失败", ex.Message);
        }
    }

    private async Task GoBackAsync()
    {
        await SaveProgressAsync();
        await SaveSessionAsync();
        _contentNavigationService.GoBack();
    }

    private async Task SaveProgressAsync()
    {
        if (Book is null)
        {
            return;
        }

        var headingIndex = Toc.Count <= 1
            ? 0
            : (int)Math.Round(ScrollRatio * (Toc.Count - 1));

        await _progressStorage.SaveAsync(new ReadingProgress
        {
            BookId = Book.Id,
            HeadingIndex = Math.Clamp(headingIndex, 0, Math.Max(0, Toc.Count - 1)),
            OffsetRatio = Math.Clamp(ScrollRatio, 0, 1),
            UpdatedAt = DateTime.Now
        });
    }

    private async Task SaveSessionAsync()
    {
        var minutes = (int)Math.Round((DateTime.Now - _sessionStart).TotalMinutes);
        if (minutes < 1)
        {
            return;
        }

        await _sessionStorage.AddAsync(new ReadingSession
        {
            BookId = Book.Id,
            StartedAt = _sessionStart,
            EndedAt = DateTime.Now,
            Minutes = minutes
        });
    }
}
