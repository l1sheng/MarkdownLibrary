using MarkdownLibrary.Library.Helpers;
using Xunit;

namespace MarkdownLibrary.UnitTest;

public class TextStatisticsTest
{
    [Fact]
    public void CountWords_ChineseOnly_CountsCharacters()
    {
        Assert.Equal(4, TextStatistics.CountWords("学而时习"));
    }

    [Fact]
    public void CountWords_MixedText_CountsChineseAndEnglishWords()
    {
        Assert.Equal(6, TextStatistics.CountWords("学而时习 markdown reader"));
    }

    [Fact]
    public void CountWords_Empty_ReturnsZero()
    {
        Assert.Equal(0, TextStatistics.CountWords("   "));
        Assert.Equal(0, TextStatistics.CountWords(null));
    }
}
