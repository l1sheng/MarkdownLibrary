using Avalonia.Controls;
using Avalonia.Interactivity;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Views;

public partial class SearchResultView : UserControl
{
    public SearchResultView()
    {
        InitializeComponent();
    }

    private void OnHitClick(object sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: SearchHit hit } && DataContext is SearchResultViewModel viewModel)
        {
            viewModel.OpenHitCommand.Execute(hit);
        }
    }
}
