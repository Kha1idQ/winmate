using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinMate.Services;

public static class UiHelpers
{
    // Two WPF-UI NavigationView quirks, fixed together once the page is really
    // in the visual tree (which is why this runs on Loaded, per page):
    //
    // 1. The host wraps pages in its own ScrollViewer and measures them with
    //    unlimited height, so a page's inner ScrollViewer never activates
    //    (lepoco/wpfui#1041). Disabling that host scroller restores scrolling.
    // 2. The content host paints its own plate (a neutral grey in the dark
    //    theme) which hides the app background. Clearing every painted layer
    //    between the page and the NavigationView lets our background show.
    // WPF-UI's shell paints structural plates (navigation pane, content host)
    // with its own defaults: a grey in the dark theme layered over a white base.
    // Neither colour belongs in this app, so we repaint any such plate with the
    // app background — that keeps one flat canvas no matter which theme is on.
    private static readonly Color[] LibraryPlateColors =
    [
        Color.FromRgb(0x20, 0x20, 0x20),
        Color.FromRgb(0xFF, 0xFF, 0xFF),
    ];

    public static void ClearLibraryPlates(FrameworkElement root)
    {
        root.Loaded += (_, _) => root.Dispatcher.BeginInvoke(
            new Action(() => RepaintPlatesRecursive(root)),
            System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private static void RepaintPlatesRecursive(DependencyObject element)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);

            if (GetBackground(child) is SolidColorBrush brush && LibraryPlateColors.Contains(brush.Color))
                SetBackgroundResource(child, "AppBackgroundBrush");

            RepaintPlatesRecursive(child);
        }
    }

    private static Brush? GetBackground(DependencyObject element) => element switch
    {
        Border border => border.Background,
        Panel panel => panel.Background,
        Control control => control.Background,
        _ => null,
    };

    private static void SetBackgroundResource(DependencyObject element, string key)
    {
        switch (element)
        {
            case Border border:
                border.SetResourceReference(Border.BackgroundProperty, key);
                break;
            case Panel panel:
                panel.SetResourceReference(Panel.BackgroundProperty, key);
                break;
            case Control control:
                control.SetResourceReference(Control.BackgroundProperty, key);
                break;
        }
    }

    private static void ClearBackground(DependencyObject element)
    {
        switch (element)
        {
            case Border border:
                border.Background = Brushes.Transparent;
                border.BorderBrush = Brushes.Transparent;
                break;
            case Panel panel:
                panel.Background = Brushes.Transparent;
                break;
            case Control control:
                control.Background = Brushes.Transparent;
                break;
        }
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
                return match;
            if (FindDescendant<T>(child) is T deeper)
                return deeper;
        }
        return null;
    }

    public static void PreparePageHost(Page page)
    {
        page.Loaded += (_, _) =>
        {
            DependencyObject? current = page;
            var scrollingDisabled = false;

            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is null or Wpf.Ui.Controls.NavigationView)
                    break;

                if (!scrollingDisabled && current is ScrollViewer host)
                {
                    host.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    host.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollingDisabled = true;
                }

                ClearBackground(current);
            }
        };
    }
}
