using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinMate.Services;

public static class UiHelpers
{
    // The NavigationView template paints its content area with its own
    // (slightly lighter) background layer. Clearing it lets the single
    // unified window color show through everywhere.
    public static void FlattenNavigationBackground(DependencyObject navigationView)
    {
        var presenter = FindDescendant<Wpf.Ui.Controls.NavigationViewContentPresenter>(navigationView);
        if (presenter is null)
            return;

        DependencyObject? current = presenter;
        while (current != null)
        {
            current = VisualTreeHelper.GetParent(current);
            if (current is Border border)
            {
                border.Background = Brushes.Transparent;
                border.BorderBrush = Brushes.Transparent;
                break;
            }
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

    // WPF-UI's NavigationView hosts pages inside its own hidden ScrollViewer,
    // which measures pages with unlimited height, so a page's own ScrollViewer
    // never activates (lepoco/wpfui#1041). Disabling that host ScrollViewer
    // gives the page the real viewport height and inner scrolling works.
    public static void DisableHostScrolling(Page page)
    {
        page.Loaded += (_, _) =>
        {
            DependencyObject? current = page;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is ScrollViewer host)
                {
                    host.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    host.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    break;
                }
            }
        };
    }
}
