using Avalonia.Controls;
using Avalonia.Interactivity;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Views;

public partial class NotesView : UserControl
{
    public NotesView()
    {
        InitializeComponent();
    }

    private void OnNoteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: NoteListItem item } && DataContext is NotesViewModel viewModel)
        {
            viewModel.OpenNoteCommand.Execute(item);
        }
    }
}
