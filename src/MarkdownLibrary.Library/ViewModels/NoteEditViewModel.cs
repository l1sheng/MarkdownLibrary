using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 新建 / 编辑一条笔记。SetParameter 接收 NoteEditContext。
/// </summary>
public class NoteEditViewModel : ViewModelBase
{
    private readonly INoteStorage _noteStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IAlertService _alertService;

    private Book _book;
    private Note _note;
    private bool _isNew = true;
    private string _excerpt = string.Empty;
    private string _content = string.Empty;
    private string _statusText = string.Empty;

    public NoteEditViewModel(INoteStorage noteStorage,
        IContentNavigationService contentNavigationService, IAlertService alertService)
    {
        _noteStorage = noteStorage;
        _contentNavigationService = contentNavigationService;
        _alertService = alertService;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        GoBackCommand = new RelayCommand(() => _contentNavigationService.GoBack());
    }

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

    public string BookTitle => Book?.Title ?? "未知书籍";

    public string Excerpt
    {
        get => _excerpt;
        set => SetProperty(ref _excerpt, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public bool IsNew
    {
        get => _isNew;
        private set
        {
            if (SetProperty(ref _isNew, value))
            {
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(PageTitle));
            }
        }
    }

    public bool CanDelete => !IsNew;

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public override string PageTitle => IsNew ? "写笔记" : "编辑笔记";

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand GoBackCommand { get; }

    public override void SetParameter(object parameter)
    {
        if (parameter is not NoteEditContext context)
        {
            return;
        }

        Book = context.Book;
        _note = context.Note;
        IsNew = context.Note is null || context.Note.Id == 0;

        Excerpt = context.Note?.Excerpt ?? string.Empty;
        Content = context.Note?.Content ?? string.Empty;
        StatusText = IsNew ? "记录一段原文，再写点自己的想法。" : string.Empty;
    }

    private async Task SaveAsync()
    {
        if (Book is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Excerpt) && string.IsNullOrWhiteSpace(Content))
        {
            StatusText = "摘录和想法至少写一项。";
            return;
        }

        try
        {
            if (IsNew)
            {
                await _noteStorage.AddAsync(new Note
                {
                    BookId = Book.Id,
                    Excerpt = Excerpt?.Trim() ?? string.Empty,
                    Content = Content?.Trim() ?? string.Empty,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                _note.Excerpt = Excerpt?.Trim() ?? string.Empty;
                _note.Content = Content?.Trim() ?? string.Empty;
                _note.UpdatedAt = DateTime.Now;
                await _noteStorage.UpdateAsync(_note);
            }

            _contentNavigationService.GoBack();
        }
        catch (Exception exception)
        {
            StatusText = "保存失败：" + exception.Message;
        }
    }

    private async Task DeleteAsync()
    {
        if (IsNew || _note is null)
        {
            _contentNavigationService.GoBack();
            return;
        }

        var confirmed = await _alertService.ConfirmAsync("删除笔记", "确定删除这条笔记吗？删除后无法恢复。");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await _noteStorage.DeleteAsync(_note.Id);
            _contentNavigationService.GoBack();
        }
        catch (Exception exception)
        {
            StatusText = "删除失败：" + exception.Message;
        }
    }
}
