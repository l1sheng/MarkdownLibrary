using Avalonia.Controls;
using Avalonia.Input;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
    }

    private void OnKeywordKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is SearchViewModel viewModel)
        {
            viewModel.SearchCommand.Execute(null);
        }
    }
}
