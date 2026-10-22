using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public class SqliteReadingSessionStorage : IReadingSessionStorage
{
    private readonly IDatabase _database;

    public SqliteReadingSessionStorage(IDatabase database)
    {
        _database = database;
    }

    public async Task AddAsync(ReadingSession session)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            insert into reading_sessions (book_id, started_at, ended_at, minutes)
            values (@BookId, @StartedAt, @EndedAt, @Minutes)
            """, session);
    }

    public async Task<int> GetTotalMinutesAsync()
    {
        using var connection = _database.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "select coalesce(sum(minutes), 0) from reading_sessions");
    }

    public async Task<int> GetMinutesOnAsync(DateTime date)
    {
        using var connection = _database.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("""
            select coalesce(sum(minutes), 0) from reading_sessions
            where date(started_at) = @day
            """, new { day = date.ToString("yyyy-MM-dd") });
    }

    public async Task<IReadOnlyList<DailyMinutes>> GetDailyMinutesAsync(int days)
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<DailyRow>("""
            select date(started_at) as day, sum(minutes) as minutes
            from reading_sessions
            where date(started_at) >= date('now', 'localtime', @offset)
            group by date(started_at)
            order by day
            """, new { offset = $"-{Math.Max(days - 1, 0)} days" });

        var map = rows.ToDictionary(x => x.Day, x => x.Minutes);
        var result = new List<DailyMinutes>();
        for (var i = days - 1; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            var key = date.ToString("yyyy-MM-dd");
            result.Add(new DailyMinutes
            {
                Date = date,
                Minutes = map.TryGetValue(key, out var minutes) ? minutes : 0
            });
        }

        return result;
    }

    private class DailyRow
    {
        public string Day { get; set; }

        public int Minutes { get; set; }
    }
}
