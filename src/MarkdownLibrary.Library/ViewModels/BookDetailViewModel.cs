using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 书籍详情：元数据、阅读进度、笔记数量、进入阅读器。
/// 演示 SetParameter 接收导航参数。
/// </summary>
public class BookDetailViewModel : ViewModelBase
{
    private readonly INoteStorage _noteStorage;
    private readonly ITagStorage _tagStorage;
    private readonly IReadingProgressStorage _progressStorage;
    private readonly IMarkdownService _markdownService;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IAlertService _alertService;

    private Book _book;
    private int _noteCount;
    private string _progressText = "尚未开始阅读";
    private string _preview = "正在载入…";
    private string _newTagName = string.Empty;

    public BookDetailViewModel(INoteStorage noteStorage, ITagStorage tagStorage,
        IReadingProgressStorage progressStorage,
        IMarkdownService markdownService, IContentNavigationService contentNavigationService,
        IAlertService alertService)
    {
        _noteStorage = noteStorage;
        _tagStorage = tagStorage;
        _progressStorage = progressStorage;
        _markdownService = markdownService;
        _contentNavigationService = contentNavigationService;
        _alertService = alertService;

        StartReadingCommand = new RelayCommand(StartReading);
        AddNoteCommand = new RelayCommand(AddNote);
        AddTagCommand = new AsyncRelayCommand(AddTagAsync);
        RemoveTagCommand = new AsyncRelayCommand<Tag>(RemoveTagAsync);
        GoBackCommand = new RelayCommand(() => _contentNavigationService.GoBack());

        Tags.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsTagsEmpty));
    }

    public Book Book
    {
        get => _book;
        private set
        {
            if (SetProperty(ref _book, value))
            {
                OnPropertyChanged(nameof(PageTitle));
            }
        }
    }

    public override string PageTitle => Book?.Title ?? "书籍详情";

    public int NoteCount
    {
        get => _noteCount;
        private set
        {
            if (SetProperty(ref _noteCount, value))
            {
                OnPropertyChanged(nameof(NoteCountText));
            }
        }
    }

    public string NoteCountText => $"{NoteCount} 条笔记";

    public string ProgressText
    {
        get => _progressText;
        private set => SetProperty(ref _progressText, value);
    }

    public string Preview
    {
        get => _preview;
        private set => SetProperty(ref _preview, value);
    }

    public ObservableCollection<Tag> Tags { get; } = new();

    public bool IsTagsEmpty => Tags.Count == 0;

    public string NewTagName
    {
        get => _newTagName;
        set => SetProperty(ref _newTagName, value);
    }

    public ICommand StartReadingCommand { get; }

    public ICommand AddNoteCommand { get; }

    public ICommand AddTagCommand { get; }

    public ICommand RemoveTagCommand { get; }

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
            NoteCount = await _noteStorage.CountByBookAsync(Book.Id);
            await LoadTagsAsync();

            var progress = await _progressStorage.GetAsync(Book.Id);
            ProgressText = progress is null
                ? "尚未开始阅读"
                : $"上次读到第 {progress.HeadingIndex + 1} 节，章内 {progress.OffsetRatio:P0}";

            var document = await _markdownService.LoadAsync(Book.FilePath);
            var text = document.PlainText;
            Preview = text.Length <= 320 ? text : text.Substring(0, 320) + "…";
        }
        catch (Exception ex)
        {
            Preview = "无法读取这本书的内容。";
            await _alertService.ShowAsync("读取失败", ex.Message);
        }
    }

    private void StartReading()
    {
        if (Book is null)
        {
            return;
        }

        _contentNavigationService.NavigateTo(ContentNavigationConstant.Reader, Book);
    }

    private void AddNote()
    {
        if (Book is null)
        {
            return;
        }

        _contentNavigationService.NavigateTo(ContentNavigationConstant.NoteEdit,
            new NoteEditContext { Book = Book });
    }

    private async Task LoadTagsAsync()
    {
        var tags = await _tagStorage.GetByBookAsync(Book.Id);
        Tags.Clear();
        foreach (var tag in tags)
        {
            Tags.Add(tag);
        }
    }

    private async Task AddTagAsync()
    {
        if (Book is null || string.IsNullOrWhiteSpace(NewTagName))
        {
            return;
        }

        try
        {
            var name = NewTagName.Trim();
            var tagId = await _tagStorage.AddAsync(name);

            if (Tags.All(tag => tag.Id != tagId))
            {
                var ids = Tags.Select(tag => tag.Id).Append(tagId);
                await _tagStorage.SetBookTagsAsync(Book.Id, ids);
            }

            NewTagName = string.Empty;
            await LoadTagsAsync();
        }
        catch (Exception exception)
        {
            await _alertService.ShowAsync("标签保存失败", exception.Message);
        }
    }

    private async Task RemoveTagAsync(Tag tag)
    {
        if (Book is null || tag is null)
        {
            return;
        }

        try
        {
            await _tagStorage.RemoveFromBookAsync(Book.Id, tag.Id);
            Tags.Remove(tag);
        }
        catch (Exception exception)
        {
            await _alertService.ShowAsync("标签删除失败", exception.Message);
        }
    }
}
