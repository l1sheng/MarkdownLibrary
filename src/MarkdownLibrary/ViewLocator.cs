using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary;

/// <summary>
/// 按命名约定把 ViewModel 映射到 View：
/// MarkdownLibrary.Library.ViewModels.ShelfViewModel -> MarkdownLibrary.Views.ShelfView
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName!
            .Replace("ViewModel", "View", StringComparison.Ordinal)
            .Replace("MarkdownLibrary.Library.", "MarkdownLibrary.");

        var type = Type.GetType(name);
        if (type is null)
        {
            return new TextBlock { Text = "Not Found: " + name };
        }

        return (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object data) => data is ViewModelBase;
}
