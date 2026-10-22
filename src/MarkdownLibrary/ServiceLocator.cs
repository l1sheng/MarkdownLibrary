using System;
using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Library.ViewModels;
using MarkdownLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MarkdownLibrary;

/// <summary>
/// 自实现的轻量 ServiceLocator：
/// 用 ServiceCollection 集中组装接口与实现，UI 只从这里取 ViewModel。
/// </summary>
public class ServiceLocator
{
    private static ServiceLocator _current;
    private IServiceProvider _provider;

    public static ServiceLocator Current =>
        _current ?? throw new InvalidOperationException("ServiceLocator 尚未初始化。");

    public MainWindowViewModel MainWindowViewModel => _provider.GetRequiredService<MainWindowViewModel>();

    public MainViewModel MainViewModel => _provider.GetRequiredService<MainViewModel>();

    public ShelfViewModel ShelfViewModel => _provider.GetRequiredService<ShelfViewModel>();

    public BookDetailViewModel BookDetailViewModel => _provider.GetRequiredService<BookDetailViewModel>();

    public ReaderViewModel ReaderViewModel => _provider.GetRequiredService<ReaderViewModel>();

    public SettingsViewModel SettingsViewModel => _provider.GetRequiredService<SettingsViewModel>();

    public NotesViewModel NotesViewModel => _provider.GetRequiredService<NotesViewModel>();

    public NoteEditViewModel NoteEditViewModel => _provider.GetRequiredService<NoteEditViewModel>();

    public SearchViewModel SearchViewModel => _provider.GetRequiredService<SearchViewModel>();

    public SearchResultViewModel SearchResultViewModel => _provider.GetRequiredService<SearchResultViewModel>();

    public StatsViewModel StatsViewModel => _provider.GetRequiredService<StatsViewModel>();

    public IMenuNavigationService MenuNavigationService => _provider.GetRequiredService<IMenuNavigationService>();

    public IRootNavigationService RootNavigationService => _provider.GetRequiredService<IRootNavigationService>();

    public IBookStorage BookStorage => _provider.GetRequiredService<IBookStorage>();

    public void Initialize()
    {
        var services = new ServiceCollection();

        // 数据层
        services.AddSingleton<IDatabase, SqliteDatabase>();
        services.AddSingleton<IBookStorage, SqliteBookStorage>();
        services.AddSingleton<INoteStorage, SqliteNoteStorage>();
        services.AddSingleton<ITagStorage, SqliteTagStorage>();
        services.AddSingleton<IReadingProgressStorage, SqliteReadingProgressStorage>();
        services.AddSingleton<IReadingSessionStorage, SqliteReadingSessionStorage>();

        // 业务服务
        services.AddSingleton<IMarkdownService, MarkdownService>();
        services.AddSingleton<ILibraryScanService, LibraryScanService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IPreferenceStorage, JsonPreferenceStorage>();
        services.AddSingleton<IAlertService, AlertService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();

        // 三层导航
        services.AddSingleton<IRootNavigationService, RootNavigationService>();
        services.AddSingleton<IMenuNavigationService, MenuNavigationService>();
        services.AddSingleton<IContentNavigationService, ContentNavigationService>();

        // ViewModel
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ShelfViewModel>();
        services.AddSingleton<BookDetailViewModel>();
        services.AddSingleton<ReaderViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<NotesViewModel>();
        services.AddSingleton<NoteEditViewModel>();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<SearchResultViewModel>();
        services.AddSingleton<StatsViewModel>();

        _provider = services.BuildServiceProvider();
        _current = this;

        _provider.GetRequiredService<IDatabase>().Initialize();
        _provider.GetRequiredService<MainWindowViewModel>();
    }
}
