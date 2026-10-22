using System;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 统计：书籍数、笔记数、累计阅读、连续天数与最近 7 天柱状图。
/// </summary>
public class StatsViewModel : ViewModelBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IAlertService _alertService;

    private StatisticsOverview _overview = new();
    private string _statusText = string.Empty;

    public StatsViewModel(IStatisticsService statisticsService, IAlertService alertService)
    {
        _statisticsService = statisticsService;
        _alertService = alertService;
    }

    public StatisticsOverview Overview
    {
        get => _overview;
        private set
        {
            if (SetProperty(ref _overview, value))
            {
                OnPropertyChanged(nameof(IsEmpty));
            }
        }
    }

    public bool IsEmpty => Overview.BookCount == 0 && Overview.NoteCount == 0 &&
                           Overview.TotalMinutes == 0;

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public override string PageTitle => "统计";

    public void Activate() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        try
        {
            Overview = await _statisticsService.GetOverviewAsync();
            StatusText = $"更新于 {DateTime.Now:HH:mm}";
        }
        catch (Exception exception)
        {
            StatusText = "统计加载失败";
            await _alertService.ShowAsync("统计加载失败", exception.Message);
        }
    }
}
