using System.Threading.Tasks;
using Dapper;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public class SqliteReadingProgressStorage : IReadingProgressStorage
{
    private readonly IDatabase _database;

    public SqliteReadingProgressStorage(IDatabase database)
    {
        _database = database;
    }

    public async Task<ReadingProgress> GetAsync(int bookId)
    {
        using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ReadingProgress>(
            "select * from reading_progress where book_id = @bookId", new { bookId });
    }

    public async Task SaveAsync(ReadingProgress progress)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            insert into reading_progress (book_id, heading_index, offset_ratio, updated_at)
            values (@BookId, @HeadingIndex, @OffsetRatio, @UpdatedAt)
            on conflict (book_id) do update set
                heading_index = excluded.heading_index,
                offset_ratio = excluded.offset_ratio,
                updated_at = excluded.updated_at
            """, progress);
    }
}
