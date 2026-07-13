using System.Windows.Controls;
using WinMate.Data;
using WinMate.Services;

namespace WinMate.Views.Pages;

public partial class TweaksPage : Page
{
    public TweaksPage()
    {
        InitializeComponent();
        UiHelpers.DisableHostScrolling(this);
        TweakList.Load(TweakCatalog.Categories, TweakCatalog.All);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TweakList.Filter(SearchBox.Text);
    }
}
