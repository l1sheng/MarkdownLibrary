namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 窗口级 ViewModel：目前只承载主界面，后续可接初始化页。
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _content;

    public MainWindowViewModel(MainViewModel main)
    {
        Main = main;
        Content = main;
    }

    public MainViewModel Main { get; }

    public ViewModelBase Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}
