using System;
using MarkdownLibrary.Library.Services;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Services;

public class MenuNavigationService : IMenuNavigationService
{
    public void NavigateTo(string pageKey)
    {
        var locator = ServiceLocator.Current;

        ViewModelBase viewModel = pageKey switch
        {
            MenuNavigationConstant.Shelf => locator.ShelfViewModel,
            MenuNavigationConstant.Notes => locator.NotesViewModel,
            MenuNavigationConstant.Search => locator.SearchViewModel,
            MenuNavigationConstant.Stats => locator.StatsViewModel,
            MenuNavigationConstant.Settings => locator.SettingsViewModel,
            _ => throw new ArgumentException($"未知的菜单：{pageKey}")
        };

        locator.MainViewModel.SetMenuAndContent(pageKey, viewModel);

        switch (pageKey)
        {
            case MenuNavigationConstant.Shelf:
                locator.ShelfViewModel.Activate();
                break;
            case MenuNavigationConstant.Notes:
                locator.NotesViewModel.Activate();
                break;
            case MenuNavigationConstant.Stats:
                locator.StatsViewModel.Activate();
                break;
            case MenuNavigationConstant.Settings:
                locator.SettingsViewModel.Activate();
                break;
        }
    }
}
