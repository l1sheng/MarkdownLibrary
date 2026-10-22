using System;
using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Services;

public class ContentNavigationService : IContentNavigationService
{
    public void NavigateTo(string pageKey, object parameter = null)
    {
        var locator = ServiceLocator.Current;

        ViewModelBase viewModel = pageKey switch
        {
            ContentNavigationConstant.BookDetail => locator.BookDetailViewModel,
            ContentNavigationConstant.Reader => locator.ReaderViewModel,
            ContentNavigationConstant.NoteEdit => locator.NoteEditViewModel,
            ContentNavigationConstant.SearchResult => locator.SearchResultViewModel,
            _ => throw new ArgumentException($"未知的内容页：{pageKey}")
        };

        if (parameter is not null)
        {
            viewModel.SetParameter(parameter);
        }

        locator.MainViewModel.PushContent(viewModel);
    }

    public void GoBack()
    {
        var mainViewModel = ServiceLocator.Current.MainViewModel;
        mainViewModel.GoBack();

        // 从阅读器返回详情时刷新一次，保证“阅读进度 / 状态”是最新的。
        if (mainViewModel.Content is BookDetailViewModel detail && detail.Book is not null)
        {
            detail.SetParameter(detail.Book);
        }

        // 从笔记编辑页返回笔记列表时重新加载。
        if (mainViewModel.Content is NotesViewModel notes)
        {
            notes.Activate();
        }
    }
}
