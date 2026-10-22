using System.Text.RegularExpressions;

namespace MarkdownLibrary.Library.Helpers;

/// <summary>
/// 字数统计：中文按字计，英文/数字按词计。
/// </summary>
public static class TextStatistics
{
    private static readonly Regex CjkRegex = new(@"[\u4e00-\u9fff]", RegexOptions.Compiled);
    private static readonly Regex WordRegex = new(@"[A-Za-z0-9]+", RegexOptions.Compiled);

    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var cjk = CjkRegex.Matches(text).Count;
        var words = WordRegex.Matches(text).Count;
        return cjk + words;
    }
}
