using CommunityToolkit.Mvvm.ComponentModel;

namespace MarkdownLibrary.Library.ViewModels;

/// <summary>
/// 所有 ViewModel 的基类。
/// 导航层在切换页面前调用 SetParameter，把“谁传给我”与“我要做什么”解耦。
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    /// <summary>页面标题，供主界面顶部栏显示。</summary>
    public virtual string PageTitle => "Markdown 图书馆";

    public virtual void SetParameter(object parameter)
    {
    }
}
