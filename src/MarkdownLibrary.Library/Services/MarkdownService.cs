using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownLibrary.Library.Helpers;
using MarkdownLibrary.Library.Models;

namespace MarkdownLibrary.Library.Services;

/// <summary>
/// Markdown 解析：front matter、目录、纯文本、字数。
/// </summary>
public class MarkdownService : IMarkdownService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public async Task<BookDocument> LoadAsync(string filePath)
    {
        var raw = await ReadRawAsync(filePath);
        var (metadata, body) = SplitFrontMatter(raw);

        if (string.IsNullOrWhiteSpace(metadata.Title))
        {
            metadata.Title = Path.GetFileNameWithoutExtension(filePath);
        }

        var document = Markdown.Parse(body, Pipeline);
        var plainText = ExtractPlainText(document);

        return new BookDocument
        {
            Metadata = metadata,
            Markdown = body,
            PlainText = plainText,
            WordCount = TextStatistics.CountWords(plainText),
            Toc = BuildToc(document)
        };
    }

    public async Task<string> ReadRawAsync(string filePath)
    {
        var text = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        return text.TrimStart('\uFEFF');
    }

    private static (BookMetadata Metadata, string Body) SplitFrontMatter(string raw)
    {
        var metadata = new BookMetadata();
        var normalized = raw.Replace("\r\n", "\n");

        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (metadata, normalized);
        }

        var end = normalized.IndexOf("\n---", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return (metadata, normalized);
        }

        var header = normalized.Substring(4, end - 4);
        var body = normalized.Substring(end + 4).TrimStart('\n');

        foreach (var line in header.Split('\n'))
        {
            var colon = line.IndexOf(':');
            if (colon <= 0)
            {
                continue;
            }

            var key = line.Substring(0, colon).Trim().ToLowerInvariant();
            var value = line.Substring(colon + 1).Trim().Trim('"');

            switch (key)
            {
                case "title":
                    metadata.Title = value;
                    break;
                case "author":
                    metadata.Author = value;
                    break;
                case "cover":
                    metadata.Cover = value;
                    break;
                case "tags":
                    metadata.Tags = value.Trim('[', ']')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().Trim('"', '\''))
                        .Where(x => x.Length > 0)
                        .ToList();
                    break;
            }
        }

        return (metadata, body);
    }

    private static List<TocItem> BuildToc(MarkdownDocument document)
    {
        var items = new List<TocItem>();
        var index = 0;

        foreach (var heading in document.Descendants<HeadingBlock>())
        {
            items.Add(new TocItem
            {
                Index = index++,
                Level = heading.Level,
                Title = GetInlineText(heading.Inline)
            });
        }

        return items;
    }

    private static string GetInlineText(ContainerInline inline)
    {
        if (inline is null)
        {
            return string.Empty;
        }

        var text = string.Concat(inline.Descendants<LiteralInline>()
            .Select(literal => literal.Content.ToString()));
        return text.Trim();
    }

    private static string ExtractPlainText(MarkdownDocument document)
    {
        var paragraphs = new List<string>();

        foreach (var leaf in document.Descendants<LeafBlock>())
        {
            if (IsInsideTable(leaf))
            {
                continue;
            }

            switch (leaf)
            {
                case HeadingBlock heading:
                    paragraphs.Add(GetInlineText(heading.Inline));
                    break;
                case ParagraphBlock paragraph:
                    paragraphs.Add(GetInlineText(paragraph.Inline));
                    break;
                case CodeBlock code:
                    paragraphs.Add(code.Lines.ToString());
                    break;
            }
        }

        return string.Join("\n", paragraphs.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static bool IsInsideTable(Block block)
    {
        for (var parent = block.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is Table)
            {
                return true;
            }
        }

        return false;
    }
}
