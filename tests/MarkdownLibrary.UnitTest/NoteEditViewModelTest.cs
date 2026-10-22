using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Models;
using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Library.ViewModels;
using NSubstitute;
using Xunit;

namespace MarkdownLibrary.UnitTest;

public class NoteEditViewModelTest
{
    [Fact]
    public async Task Save_NewNote_AddsNoteAndNavigatesBack()
    {
        var noteStorage = Substitute.For<INoteStorage>();
        var navigation = Substitute.For<IContentNavigationService>();
        var alert = Substitute.For<IAlertService>();

        var viewModel = new NoteEditViewModel(noteStorage, navigation, alert);
        viewModel.SetParameter(new NoteEditContext
        {
            Book = new Book { Id = 7, Title = "测试书" }
        });
        viewModel.Excerpt = "学而时习之";
        viewModel.Content = "复习很重要。";

        await ((IAsyncRelayCommand)viewModel.SaveCommand).ExecuteAsync(null);

        await noteStorage.Received(1).AddAsync(Arg.Is<Note>(note =>
            note.BookId == 7 &&
            note.Excerpt == "学而时习之" &&
            note.Content == "复习很重要。"));
        navigation.Received(1).GoBack();
    }

    [Fact]
    public async Task Save_EmptyContent_DoesNotAddNote()
    {
        var noteStorage = Substitute.For<INoteStorage>();
        var navigation = Substitute.For<IContentNavigationService>();
        var alert = Substitute.For<IAlertService>();

        var viewModel = new NoteEditViewModel(noteStorage, navigation, alert);
        viewModel.SetParameter(new NoteEditContext
        {
            Book = new Book { Id = 1, Title = "书" }
        });
        viewModel.Excerpt = "   ";
        viewModel.Content = string.Empty;

        await ((IAsyncRelayCommand)viewModel.SaveCommand).ExecuteAsync(null);

        await noteStorage.DidNotReceive().AddAsync(Arg.Any<Note>());
        navigation.DidNotReceive().GoBack();
        Assert.Equal("摘录和想法至少写一项。", viewModel.StatusText);
    }

    [Fact]
    public async Task Delete_ExistingNote_ConfirmsAndDeletes()
    {
        var noteStorage = Substitute.For<INoteStorage>();
        var navigation = Substitute.For<IContentNavigationService>();
        var alert = Substitute.For<IAlertService>();
        alert.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var viewModel = new NoteEditViewModel(noteStorage, navigation, alert);
        viewModel.SetParameter(new NoteEditContext
        {
            Book = new Book { Id = 2, Title = "书" },
            Note = new Note { Id = 99, BookId = 2, Content = "旧笔记" }
        });

        await ((IAsyncRelayCommand)viewModel.DeleteCommand).ExecuteAsync(null);

        await noteStorage.Received(1).DeleteAsync(99);
        navigation.Received(1).GoBack();
    }
}
