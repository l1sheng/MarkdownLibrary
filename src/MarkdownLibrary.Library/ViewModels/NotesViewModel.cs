using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 全部笔记：按更新时间倒序展示，点击进入编辑。
/// </summary>
public class NotesViewModel : ViewModelBase
{
    private readonly INoteStorage _noteStorage;
    private readonly IBookStorage _bookStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IAlertService _alertService;

    private string _statusText = "正在加载…";

    public NotesViewModel(INoteStorage noteStorage, IBookStorage bookStorage,
        IContentNavigationService contentNavigationService, IAlertService alertService)
    {
        _noteStorage = noteStorage;
        _bookStorage = bookStorage;
        _contentNavigationService = contentNavigationService;
        _alertService = alertService;

        OpenNoteCommand = new AsyncRelayCommand<NoteListItem>(OpenNoteAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);

        Notes.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(CountText));
        };
    }

    public ObservableCollection<NoteListItem> Notes { get; } = new();

    public bool IsEmpty => Notes.Count == 0;

    public string CountText => $"共 {Notes.Count} 条";

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public override string PageTitle => "笔记";

    public ICommand OpenNoteCommand { get; }

    public ICommand RefreshCommand { get; }

    public void Activate() => _ = LoadAsync();

    public async Task LoadAsync()
    {
        try
        {
            var notes = await _noteStorage.GetAllWithBookAsync();
            Notes.Clear();
            foreach (var note in notes)
            {
                Notes.Add(note);
            }

            StatusText = Notes.Count == 0
                ? "还没有笔记。打开一本书，在详情页点“写笔记”。"
                : $"共 {Notes.Count} 条笔记";
        }
        catch (Exception exception)
        {
            StatusText = "笔记加载失败";
            await _alertService.ShowAsync("加载失败", exception.Message);
        }
    }

    private async Task OpenNoteAsync(NoteListItem item)
    {
        if (item is null)
        {
            return;
        }

        var book = await _bookStorage.GetByIdAsync(item.BookId);
        if (book is null)
        {
            await _alertService.ShowAsync("打开失败", "这条笔记对应的书已经不在书库里了。");
            return;
        }

        _contentNavigationService.NavigateTo(ContentNavigationConstant.NoteEdit,
            new NoteEditContext { Book = book, Note = item });
    }
}
