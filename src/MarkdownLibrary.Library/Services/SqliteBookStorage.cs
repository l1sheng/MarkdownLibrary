using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public class SqliteBookStorage : IBookStorage
{
    private readonly IDatabase _database;

    public SqliteBookStorage(IDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync()
    {
        using var connection = _database.CreateConnection();
        var rows = await connection.QueryAsync<Book>(
            "select * from books order by coalesce(last_opened_at, created_at) desc, id desc");
        return rows.ToList();
    }

    public async Task<Book> GetByIdAsync(int id)
    {
        using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Book>(
            "select * from books where id = @id", new { id });
    }

    public async Task<Book> GetByPathAsync(string filePath)
    {
        using var connection = _database.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Book>(
            "select * from books where file_path = @filePath", new { filePath });
    }

    public async Task<int> AddAsync(Book book)
    {
        using var connection = _database.CreateConnection();
        var id = await connection.ExecuteScalarAsync<long>("""
            insert into books (title, author, file_path, cover_path, word_count, status, created_at, last_opened_at)
            values (@Title, @Author, @FilePath, @CoverPath, @WordCount, @Status, @CreatedAt, @LastOpenedAt);
            select last_insert_rowid();
            """, book);
        return (int)id;
    }

    public async Task UpdateAsync(Book book)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            update books
            set title = @Title,
                author = @Author,
                file_path = @FilePath,
                cover_path = @CoverPath,
                word_count = @WordCount,
                status = @Status,
                last_opened_at = @LastOpenedAt
            where id = @Id
            """, book);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("delete from books where id = @id", new { id });
    }

    public async Task UpdateLastOpenedAsync(int id, DateTime time)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "update books set last_opened_at = @time where id = @id", new { id, time });
    }

    public async Task UpdateStatusAsync(int id, BookStatus status)
    {
        using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "update books set status = @status where id = @id", new { id, status });
    }
}
