using System;
using System.Linq;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;

var database = new SqliteDatabase();
database.Initialize();

var bookStorage = new SqliteBookStorage(database);
var noteStorage = new SqliteNoteStorage(database);
var progressStorage = new SqliteReadingProgressStorage(database);
var sessionStorage = new SqliteReadingSessionStorage(database);
var markdownService = new MarkdownService();
var scanService = new LibraryScanService(bookStorage, markdownService);

var folder = args.Length > 0 ? args[0] : "samples";
var result = await scanService.ScanAsync(folder);
Console.WriteLine($"scan: added={result.Added} updated={result.Updated} skipped={result.Skipped}");

var books = await bookStorage.GetAllAsync();
foreach (var book in books)
{
    Console.WriteLine($"book: {book.Id} | {book.Title} | {book.Author} | {book.WordCount} | {book.StatusText}");
}

if (books.Count == 0)
{
    Console.WriteLine("no books found");
    return;
}

var first = books[0];
var document = await markdownService.LoadAsync(first.FilePath);
Console.WriteLine($"doc: title={document.Metadata.Title} toc={document.Toc.Count} words={document.WordCount}");
foreach (var item in document.Toc.Take(5))
{
    Console.WriteLine($"  toc[{item.Index}] h{item.Level} {item.Title}");
}

await progressStorage.SaveAsync(new ReadingProgress
{
    BookId = first.Id,
    HeadingIndex = 2,
    OffsetRatio = 0.42,
    UpdatedAt = DateTime.Now
});

var progress = await progressStorage.GetAsync(first.Id);
Console.WriteLine($"progress: heading={progress?.HeadingIndex} ratio={progress?.OffsetRatio} updated={progress?.UpdatedAt:O}");

await sessionStorage.AddAsync(new ReadingSession
{
    BookId = first.Id,
    StartedAt = DateTime.Now.AddMinutes(-20),
    EndedAt = DateTime.Now,
    Minutes = 20
});

var total = await sessionStorage.GetTotalMinutesAsync();
var today = await sessionStorage.GetMinutesOnAsync(DateTime.Today);
var daily = await sessionStorage.GetDailyMinutesAsync(7);
Console.WriteLine($"sessions: total={total} today={today} days={daily.Count} last={daily.Last().Minutes}");

var noteId = await noteStorage.AddAsync(new Note
{
    BookId = first.Id,
    Excerpt = "学而时习之",
    Content = "复习很重要。",
    CreatedAt = DateTime.Now,
    UpdatedAt = DateTime.Now
});
Console.WriteLine($"note: id={noteId} count={await noteStorage.CountByBookAsync(first.Id)}");

var statistics = new StatisticsService(bookStorage, noteStorage, sessionStorage);
var overview = await statistics.GetOverviewAsync();
Console.WriteLine($"stats: books={overview.BookCount} notes={overview.NoteCount} total={overview.TotalMinutes} streak={overview.StreakDays}");
