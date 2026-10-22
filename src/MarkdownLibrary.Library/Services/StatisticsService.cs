using System;
using System.Linq;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IBookStorage _bookStorage;
    private readonly INoteStorage _noteStorage;
    private readonly IReadingSessionStorage _sessionStorage;

    public StatisticsService(IBookStorage bookStorage, INoteStorage noteStorage,
        IReadingSessionStorage sessionStorage)
    {
        _bookStorage = bookStorage;
        _noteStorage = noteStorage;
        _sessionStorage = sessionStorage;
    }

    public async Task<StatisticsOverview> GetOverviewAsync()
    {
        var books = await _bookStorage.GetAllAsync();
        var notes = await _noteStorage.CountAllAsync();
        var totalMinutes = await _sessionStorage.GetTotalMinutesAsync();
        var todayMinutes = await _sessionStorage.GetMinutesOnAsync(DateTime.Today);
        var daily = await _sessionStorage.GetDailyMinutesAsync(7);

        var streak = 0;
        foreach (var day in daily.Reverse())
        {
            if (day.Minutes <= 0)
            {
                break;
            }

            streak++;
        }

        return new StatisticsOverview
        {
            BookCount = books.Count,
            NoteCount = notes,
            TotalMinutes = totalMinutes,
            TodayMinutes = todayMinutes,
            StreakDays = streak,
            LastSevenDays = daily
        };
    }
}
