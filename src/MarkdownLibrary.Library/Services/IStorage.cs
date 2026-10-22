using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

public interface IBookStorage
{
    Task<IReadOnlyList<Book>> GetAllAsync();

    Task<Book> GetByIdAsync(int id);

    Task<Book> GetByPathAsync(string filePath);

    Task<int> AddAsync(Book book);

    Task UpdateAsync(Book book);

    Task DeleteAsync(int id);

    Task UpdateLastOpenedAsync(int id, DateTime time);

    Task UpdateStatusAsync(int id, BookStatus status);
}

public interface INoteStorage
{
    Task<IReadOnlyList<Note>> GetAllAsync();

    Task<IReadOnlyList<NoteListItem>> GetAllWithBookAsync();

    Task<Note> GetByIdAsync(int id);

    Task<IReadOnlyList<Note>> GetByBookAsync(int bookId);

    Task<int> AddAsync(Note note);

    Task UpdateAsync(Note note);

    Task DeleteAsync(int id);

    Task<int> CountByBookAsync(int bookId);

    Task<int> CountAllAsync();
}

public interface ITagStorage
{
    Task<IReadOnlyList<Tag>> GetAllAsync();

    Task<int> AddAsync(string name);

    Task<IReadOnlyList<Tag>> GetByBookAsync(int bookId);

    Task SetBookTagsAsync(int bookId, IEnumerable<int> tagIds);

    Task RemoveFromBookAsync(int bookId, int tagId);
}

public interface IReadingProgressStorage
{
    Task<ReadingProgress> GetAsync(int bookId);

    Task SaveAsync(ReadingProgress progress);
}

public interface IReadingSessionStorage
{
    Task AddAsync(ReadingSession session);

    Task<int> GetTotalMinutesAsync();

    Task<int> GetMinutesOnAsync(DateTime date);

    Task<IReadOnlyList<DailyMinutes>> GetDailyMinutesAsync(int days);
}
