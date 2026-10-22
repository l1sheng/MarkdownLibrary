using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Helpers;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 设置：书库目录、重新扫描。
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly IPreferenceStorage _preferenceStorage;
    private readonly IFileDialogService _fileDialogService;
    private readonly ILibraryScanService _scanService;
    private readonly IAlertService _alertService;

    private string _libraryFolder;
    private string _statusText = string.Empty;

    public SettingsViewModel(IPreferenceStorage preferenceStorage,
        IFileDialogService fileDialogService, ILibraryScanService scanService,
        IAlertService alertService)
    {
        _preferenceStorage = preferenceStorage;
        _fileDialogService = fileDialogService;
        _scanService = scanService;
        _alertService = alertService;

        ChooseFolderCommand = new AsyncRelayCommand(ChooseFolderAsync);
        RescanCommand = new AsyncRelayCommand(RescanAsync);

        _libraryFolder = _preferenceStorage.Get(PreferenceKeys.LibraryFolder,
            PathHelper.DefaultLibraryDirectory);
    }

    public string LibraryFolder
    {
        get => _libraryFolder;
        private set => SetProperty(ref _libraryFolder, value);
    }

    public override string PageTitle => "设置";

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public ICommand ChooseFolderCommand { get; }

    public ICommand RescanCommand { get; }

    public void Activate()
    {
        LibraryFolder = _preferenceStorage.Get(PreferenceKeys.LibraryFolder,
            PathHelper.DefaultLibraryDirectory);
    }

    private async Task ChooseFolderAsync()
    {
        var folder = await _fileDialogService.PickFolderAsync();
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        _preferenceStorage.Set(PreferenceKeys.LibraryFolder, folder);
        LibraryFolder = folder;
        await ScanAsync();
    }

    private async Task RescanAsync() => await ScanAsync();

    private async Task ScanAsync()
    {
        try
        {
            StatusText = "正在扫描…";
            var result = await _scanService.ScanAsync(LibraryFolder);
            StatusText = $"扫描完成：新增 {result.Added} 本，更新 {result.Updated} 本，跳过 {result.Skipped} 个文件";
        }
        catch (Exception ex)
        {
            StatusText = "扫描失败";
            await _alertService.ShowAsync("扫描失败", ex.Message);
        }
    }
}
