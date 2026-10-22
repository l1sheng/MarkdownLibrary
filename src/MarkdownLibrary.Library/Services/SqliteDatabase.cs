using System.IO;
using Dapper;
using MarkdownLibrary.Library.Helpers;
using Microsoft.Data.Sqlite;

namespace MarkdownLibrary.Library.Services;

/// <summary>
/// SQLite 连接工厂与建表逻辑。
/// 第一次启动时自动创建数据库和全部表。
/// </summary>
public class SqliteDatabase : IDatabase
{
    private const string Schema = """
        create table if not exists books (
            id integer primary key autoincrement,
            title text not null,
            author text,
            file_path text not null unique,
            cover_path text,
            word_count integer not null default 0,
            status integer not null default 0,
            created_at text not null,
            last_opened_at text
        );

        create table if not exists notes (
            id integer primary key autoincrement,
            book_id integer not null,
            excerpt text,
            content text not null,
            created_at text not null,
            updated_at text not null,
            foreign key (book_id) references books (id) on delete cascade
        );

        create table if not exists tags (
            id integer primary key autoincrement,
            name text not null unique
        );

        create table if not exists book_tags (
            book_id integer not null,
            tag_id integer not null,
            primary key (book_id, tag_id),
            foreign key (book_id) references books (id) on delete cascade,
            foreign key (tag_id) references tags (id) on delete cascade
        );

        create table if not exists reading_progress (
            book_id integer primary key,
            heading_index integer not null default 0,
            offset_ratio real not null default 0,
            updated_at text not null,
            foreign key (book_id) references books (id) on delete cascade
        );

        create table if not exists reading_sessions (
            id integer primary key autoincrement,
            book_id integer not null,
            started_at text not null,
            ended_at text not null,
            minutes integer not null,
            foreign key (book_id) references books (id) on delete cascade
        );

        create index if not exists ix_notes_book on notes (book_id);
        create index if not exists ix_sessions_started on reading_sessions (started_at);
        create index if not exists ix_books_title on books (title);
        """;

    static SqliteDatabase()
    {
        // 让 Dapper 把 snake_case 列名映射到 PascalCase 属性。
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public SqliteDatabase()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = PathHelper.DatabasePath,
            ForeignKeys = true
        };
        ConnectionString = builder.ToString();
    }

    public string ConnectionString { get; }

    public SqliteConnection CreateConnection()
    {
        Directory.CreateDirectory(PathHelper.DataDirectory);
        return new SqliteConnection(ConnectionString);
    }

    public void Initialize()
    {
        using var connection = CreateConnection();
        connection.Open();

        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "pragma journal_mode = wal;";
            pragma.ExecuteNonQuery();
        }

        using var command = connection.CreateCommand();
        command.CommandText = Schema;
        command.ExecuteNonQuery();
    }
}
