using System;
using System.IO;
using System.Threading.Tasks;
using MarkdownLibrary.Library.Services;
using Xunit;

namespace MarkdownLibrary.UnitTest;

public class MarkdownServiceTest
{
    private readonly MarkdownService _service = new();

    [Fact]
    public async Task LoadAsync_ShouldParseFrontMatterAndToc()
    {
        var path = CreateTempFile("""
            ---
            title: 测试之书
            author: 张三
            tags: [随笔, 测试]
            ---

            # 第一章

            正文内容。

            ## 第一节

            更多内容。

            # 第二章
            """);

        try
        {
            var document = await _service.LoadAsync(path);

            Assert.Equal("测试之书", document.Metadata.Title);
            Assert.Equal("张三", document.Metadata.Author);
            Assert.Contains("随笔", document.Metadata.Tags);
            Assert.Equal(3, document.Toc.Count);
            Assert.Equal("第一章", document.Toc[0].Title);
            Assert.Equal(2, document.Toc[1].Level);
            Assert.True(document.WordCount > 0);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_WithoutFrontMatter_ShouldUseFileName()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mdlib-{Guid.NewGuid():N}.md");
        await File.WriteAllTextAsync(path, "# 只有标题\n\n一些内容。");

        try
        {
            var document = await _service.LoadAsync(path);
            Assert.Equal(Path.GetFileNameWithoutExtension(path), document.Metadata.Title);
            Assert.Single(document.Toc);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"mdlib-{Guid.NewGuid():N}.md");
        File.WriteAllText(path, content);
        return path;
    }
}
