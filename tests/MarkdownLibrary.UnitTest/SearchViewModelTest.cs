using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Library.ViewModels;
using NSubstitute;
using Xunit;

namespace MarkdownLibrary.UnitTest;

public class SearchViewModelTest
{
    [Fact]
    public void Search_WithKeyword_NavigatesToResultPage()
    {
        var navigation = Substitute.For<IContentNavigationService>();
        var viewModel = new SearchViewModel(navigation) { Keyword = "  道德经  " };

        viewModel.SearchCommand.Execute(null);

        navigation.Received(1).NavigateTo(ContentNavigationConstant.SearchResult, "道德经");
    }

    [Fact]
    public void Search_EmptyKeyword_DoesNothing()
    {
        var navigation = Substitute.For<IContentNavigationService>();
        var viewModel = new SearchViewModel(navigation) { Keyword = "   " };

        viewModel.SearchCommand.Execute(null);

        navigation.DidNotReceive().NavigateTo(Arg.Any<string>(), Arg.Any<object>());
    }
}
