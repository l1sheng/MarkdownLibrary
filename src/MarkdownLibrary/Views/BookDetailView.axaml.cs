using Avalonia.Controls;
using Avalonia.Interactivity;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Views;

public partial class BookDetailView : UserControl
{
    public BookDetailView()
    {
        InitializeComponent();
    }

    private void OnRemoveTagClick(object sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: Tag tag } && DataContext is BookDetailViewModel viewModel)
        {
            viewModel.RemoveTagCommand.Execute(tag);
        }
    }
}
