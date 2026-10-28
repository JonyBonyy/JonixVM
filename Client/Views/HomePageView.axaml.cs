using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }
    
    private void LoginButtonClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window != null)
        {
            // Instantly swap the view back to the saved start screen
            window.Content = MainWindow.StartScreenContent;
        }
    }
}