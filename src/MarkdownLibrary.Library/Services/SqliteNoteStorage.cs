using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public class SqliteNoteStorage : INoteStorage
{
    private readonly IDatabase _database;

    public SqliteNoteStorage(IDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<Note>> GetAllAsync()
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<Note>(
            "select * from notes order by updated_at desc");
        return rows.ToList();
    }

    public async Task<IReadOnlyList<NoteListItem>> GetAllWithBookAsync()
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<NoteListItem>("""
            select n.*, b.title as BookTitle
            from notes n
            join books b on b.id = n.book_id
            order by n.updated_at desc
            """);
        return rows.ToList();
    }

    public async Task<Note> GetByIdAsync(int id)
    {
        using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Note>(
            "select * from notes where id = @id", new { id });
    }

    public async Task<IReadOnlyList<Note>> GetByBookAsync(int bookId)
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<Note>(
            "select * from notes where book_id = @bookId order by updated_at desc", new { bookId });
        return rows.ToList();
    }

    public async Task<int> AddAsync(Note note)
    {
        using var connection = _database.CreateConnection();
        var id = await connection.ExecuteScalarAsync<long>("""
            insert into notes (book_id, excerpt, content, created_at, updated_at)
            values (@BookId, @Excerpt, @Content, @CreatedAt, @UpdatedAt);
            select last_insert_rowid();
            """, note);
        return (int)id;
    }

    public async Task UpdateAsync(Note note)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            update notes
            set excerpt = @Excerpt,
                content = @Content,
                updated_at = @UpdatedAt
            where id = @Id
            """, note);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("delete from notes where id = @id", new { id });
    }

    public async Task<int> CountByBookAsync(int bookId)
    {
        using var connection = _database.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "select count(*) from notes where book_id = @bookId", new { bookId });
    }

    public async Task<int> CountAllAsync()
    {
        using var connection = _database.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("select count(*) from notes");
    }
}
