using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Helpers;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 书架：展示书库、导入文件夹、重新扫描、打开详情。
/// </summary>
public class ShelfViewModel : ViewModelBase
{
    private readonly IBookStorage _bookStorage;
    private readonly ILibraryScanService _scanService;
    private readonly IPreferenceStorage _preferenceStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IAlertService _alertService;
    private readonly IFileDialogService _fileDialogService;

    private string _statusText = "正在加载书库…";
    private bool _isBusy;

    public ShelfViewModel(IBookStorage bookStorage, ILibraryScanService scanService,
        IPreferenceStorage preferenceStorage, IContentNavigationService contentNavigationService,
        IAlertService alertService, IFileDialogService fileDialogService)
    {
        _bookStorage = bookStorage;
        _scanService = scanService;
        _preferenceStorage = preferenceStorage;
        _contentNavigationService = contentNavigationService;
        _alertService = alertService;
        _fileDialogService = fileDialogService;

        RefreshCommand = new AsyncRelayCommand(RescanAsync);
        ImportFolderCommand = new AsyncRelayCommand(ImportFolderAsync);
        OpenBookCommand = new RelayCommand<Book>(OpenBook);

        Books.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(BookCountText));
        };
    }

    public ObservableCollection<Book> Books { get; } = new();

    public override string PageTitle => "书架";

    public bool IsEmpty => Books.Count == 0;

    public string BookCountText => $"共 {Books.Count} 本";

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public ICommand RefreshCommand { get; }

    public ICommand ImportFolderCommand { get; }

    public ICommand OpenBookCommand { get; }

    public void Activate()
    {
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            var books = await _bookStorage.GetAllAsync();

            Books.Clear();
            foreach (var book in books)
            {
                Books.Add(book);
            }

            var folder = _preferenceStorage.Get(PreferenceKeys.LibraryFolder,
                PathHelper.DefaultLibraryDirectory);
            StatusText = Books.Count == 0
                ? $"书库是空的。点击“导入文件夹”，选择存放 Markdown 的目录（当前：{folder}）"
                : $"{BookCountText} · 书库：{folder}";
        }
        catch (Exception ex)
        {
            StatusText = "书库加载失败";
            await _alertService.ShowAsync("加载失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RescanAsync()
    {
        var folder = _preferenceStorage.Get(PreferenceKeys.LibraryFolder,
            PathHelper.DefaultLibraryDirectory);
        await ScanAsync(folder);
    }

    private async Task ImportFolderAsync()
    {
        var folder = await _fileDialogService.PickFolderAsync();
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        _preferenceStorage.Set(PreferenceKeys.LibraryFolder, folder);
        await ScanAsync(folder);
    }

    private async Task ScanAsync(string folder)
    {
        try
        {
            IsBusy = true;
            StatusText = "正在扫描…";
            var result = await _scanService.ScanAsync(folder);
            await LoadAsync();
            StatusText = $"扫描完成：新增 {result.Added} 本，更新 {result.Updated} 本，跳过 {result.Skipped} 个文件";
        }
        catch (Exception ex)
        {
            StatusText = "扫描失败";
            await _alertService.ShowAsync("扫描失败", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenBook(Book book)
    {
        if (book is null)
        {
            return;
        }

        _contentNavigationService.NavigateTo(ContentNavigationConstant.BookDetail, book);
    }
}
