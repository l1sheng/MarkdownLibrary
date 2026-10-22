using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MarkdownLibrary.Library.Services;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 主界面：左侧菜单 + 右侧内容区，并维护内容页返回栈。
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IMenuNavigationService _menuNavigationService;

    private string _title = "Markdown 图书馆";
    private bool _isPaneOpen = true;
    private ViewModelBase _content;
    private MenuItem _selectedMenuItem;
    private string _currentMenuView;

    public MainViewModel(IMenuNavigationService menuNavigationService)
    {
        _menuNavigationService = menuNavigationService;

        OpenPaneCommand = new RelayCommand(() => IsPaneOpen = true);
        ClosePaneCommand = new RelayCommand(() => IsPaneOpen = false);
        GoBackCommand = new RelayCommand(GoBack);
        NavigateMenuCommand = new RelayCommand<MenuItem>(NavigateMenu);
        TogglePaneCommand = new RelayCommand(() => IsPaneOpen = !IsPaneOpen);
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => SetProperty(ref _isPaneOpen, value);
    }

    public ViewModelBase Content
    {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public MenuItem SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => SetProperty(ref _selectedMenuItem, value);
    }

    public IReadOnlyList<MenuItem> MenuItems => MenuItem.MenuItems;

    public ObservableCollection<ViewModelBase> ContentStack { get; } = new();

    public bool CanGoBack => ContentStack.Count > 1;

    public ICommand OpenPaneCommand { get; }

    public ICommand ClosePaneCommand { get; }

    public ICommand TogglePaneCommand { get; }

    public ICommand GoBackCommand { get; }

    public ICommand NavigateMenuCommand { get; }

    public void PushContent(ViewModelBase content)
    {
        if (content is null)
        {
            return;
        }

        ContentStack.Insert(0, content);
        Content = content;
        OnPropertyChanged(nameof(CanGoBack));
    }

    public void SetMenuAndContent(string view, ViewModelBase content)
    {
        _currentMenuView = view;
        ContentStack.Clear();
        PushContent(content);

        SelectedMenuItem = MenuItem.MenuItems.FirstOrDefault(item => item.View == view);
        Title = SelectedMenuItem?.Name ?? "Markdown 图书馆";
        IsPaneOpen = false;
    }

    public void NavigateMenu(MenuItem item)
    {
        if (item is null || item.View == _currentMenuView)
        {
            return;
        }

        _menuNavigationService.NavigateTo(item.View);
    }

    public void GoBack()
    {
        if (ContentStack.Count <= 1)
        {
            return;
        }

        ContentStack.RemoveAt(0);
        Content = ContentStack[0];
        OnPropertyChanged(nameof(CanGoBack));
    }
}
