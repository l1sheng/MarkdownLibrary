using Avalonia.Controls;
using Avalonia.Interactivity;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Views;

public partial class ShelfView : UserControl
{
    public ShelfView()
    {
        InitializeComponent();
    }

    private void OnBookClick(object sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: Book book } && DataContext is ShelfViewModel viewModel)
        {
            viewModel.OpenBookCommand.Execute(book);
        }
    }
}
