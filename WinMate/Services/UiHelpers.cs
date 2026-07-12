using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinMate.Services;

public static class UiHelpers
{
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
