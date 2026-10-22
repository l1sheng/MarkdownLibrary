using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

/// <summary>
/// 简单搜索：书名 / 作者 / 笔记内容。后续可平滑替换为 SQLite FTS5。
/// </summary>
public class SearchService : ISearchService
{
    private readonly IDatabase _database;

    public SearchService(IDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<SearchHit>();
        }

        var pattern = $"%{keyword.Trim()}%";
        using var connection = _database.CreateConnection();

        var bookHits = await connection.QueryAsync<SearchHit>("""
            select 'book' as kind, id as book_id, title as book_title, author as snippet
            from books
            where title like @pattern or author like @pattern
            limit 50
            """, new { pattern });

        var noteHits = await connection.QueryAsync<SearchHit>("""
            select 'note' as kind, b.id as book_id, n.id as note_id,
                   b.title as book_title,
                   case when length(n.content) > 0 then n.content else n.excerpt end as snippet
            from notes n
            join books b on b.id = n.book_id
            where n.content like @pattern or n.excerpt like @pattern
            limit 50
            """, new { pattern });

        return bookHits.Concat(noteHits).ToList();
    }
}
