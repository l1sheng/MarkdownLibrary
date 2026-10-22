using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public class SqliteTagStorage : ITagStorage
{
    private readonly IDatabase _database;

    public SqliteTagStorage(IDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync()
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<Tag>("select * from tags order by name");
        return rows.ToList();
    }

    public async Task<int> AddAsync(string name)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "insert or ignore into tags (name) values (@name)", new { name });
        return await connection.ExecuteScalarAsync<int>(
            "select id from tags where name = @name", new { name });
    }

    public async Task<IReadOnlyList<Tag>> GetByBookAsync(int bookId)
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<Tag>("""
            select t.* from tags t
            join book_tags bt on bt.tag_id = t.id
            where bt.book_id = @bookId
            order by t.name
            """, new { bookId });
        return rows.ToList();
    }

    public async Task SetBookTagsAsync(int bookId, IEnumerable<int> tagIds)
    {
        using var connection = _database.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync(
            "delete from book_tags where book_id = @bookId", new { bookId }, transaction);

        foreach (var tagId in tagIds.Distinct())
        {
            await connection.ExecuteAsync(
                "insert into book_tags (book_id, tag_id) values (@bookId, @tagId)",
                new { bookId, tagId }, transaction);
        }

        transaction.Commit();
    }

    public async Task RemoveFromBookAsync(int bookId, int tagId)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "delete from book_tags where book_id = @bookId and tag_id = @tagId",
            new { bookId, tagId });
    }
}
