using Avalonia.Controls;
using MarkdownLibrary.Library.ViewModels;
using MenuItem = MarkdownLibrary.Library.ViewModels.MenuItem;

namespace MarkdownLibrary.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is MenuItem item)
        {
            viewModel.NavigateMenu(item);
        }
    }
}
