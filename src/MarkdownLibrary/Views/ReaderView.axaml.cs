using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Markdown.Avalonia;
using MarkdownLibrary.Library.ViewModels;

namespace MarkdownLibrary.Views;

public partial class ReaderView : UserControl
{
    private ReaderViewModel _viewModel;
    private ScrollViewer _scrollViewer;
    private bool _pendingRestore;
    private int _pendingHeading;
    private double _pendingRatio;
    private int _attachAttempts;

    public ReaderView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as ReaderViewModel;
        if (_viewModel is null)
        {
            return;
        }

        var viewer = this.GetVisualDescendants().OfType<MarkdownScrollViewer>().FirstOrDefault();
        if (viewer is not null && !string.IsNullOrWhiteSpace(_viewModel.Book?.FilePath))
        {
            // 让 Markdown 里的相对图片路径按书籍所在目录解析。
            viewer.AssetPathRoot = Path.GetDirectoryName(_viewModel.Book.FilePath);
        }

        _viewModel.RestoreScrollRequested += OnRestoreRequested;
        _viewModel.ScrollToHeadingRequested += OnScrollToHeading;
        AttachScrollViewer();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.RestoreScrollRequested -= OnRestoreRequested;
            _viewModel.ScrollToHeadingRequested -= OnScrollToHeading;
        }

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        }
    }

    private void AttachScrollViewer()
    {
        // 注意：页面上还有一个目录 ListBox，它内部也有 ScrollViewer。
        // 必须把查找范围限定在 Markdown 控件内部，否则会滚错对象。
        _scrollViewer = FindDocumentScrollViewer();
        if (_scrollViewer is null)
        {
            _attachAttempts++;
            if (_attachAttempts < 50)
            {
                DispatcherTimer.RunOnce(AttachScrollViewer, TimeSpan.FromMilliseconds(100));
            }

            return;
        }

        _scrollViewer.ScrollChanged += OnScrollChanged;

        if (_viewModel is { DocumentReady: true })
        {
            ApplyRestore(_viewModel.RestoreHeadingIndex, _viewModel.RestoreOffsetRatio);
        }
        else if (_pendingRestore)
        {
            ApplyRestore(_pendingHeading, _pendingRatio);
        }
    }

    private ScrollViewer FindDocumentScrollViewer()
    {
        var viewer = this.FindControl<MarkdownScrollViewer>("MarkdownView");
        if (viewer is not null)
        {
            var inner = viewer.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            if (inner is not null)
            {
                return inner;
            }
        }

        return this.GetVisualDescendants().OfType<ScrollViewer>()
            .FirstOrDefault(scrollViewer => scrollViewer.FindAncestorOfType<ListBox>() is null);
    }

    private void OnRestoreRequested(int headingIndex, double ratio)
    {
        _pendingHeading = headingIndex;
        _pendingRatio = ratio;
        _pendingRestore = true;
        Dispatcher.UIThread.Post(() => ApplyRestore(headingIndex, ratio), DispatcherPriority.Background);
    }

    private void OnScrollToHeading(int headingIndex)
    {
        if (_viewModel is null || _viewModel.Toc.Count <= 1)
        {
            return;
        }

        var ratio = headingIndex / (double)(_viewModel.Toc.Count - 1);
        ApplyRestore(headingIndex, ratio);
    }

    private void ApplyRestore(int headingIndex, double ratio)
    {
        if (_scrollViewer is null)
        {
            _pendingRestore = true;
            _pendingHeading = headingIndex;
            _pendingRatio = ratio;
            return;
        }

        var max = _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height;
        if (max <= 1)
        {
            DispatcherTimer.RunOnce(() => ApplyRestore(headingIndex, ratio), TimeSpan.FromMilliseconds(250));
            return;
        }

        var target = Math.Clamp(ratio, 0, 1);
        _scrollViewer.Offset = new Vector(0, target * max);
        _pendingRestore = false;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_viewModel is null || _scrollViewer is null)
        {
            return;
        }

        var max = _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height;
        _viewModel.ScrollRatio = max <= 0 ? 0 : Math.Clamp(_scrollViewer.Offset.Y / max, 0, 1);
    }
}
