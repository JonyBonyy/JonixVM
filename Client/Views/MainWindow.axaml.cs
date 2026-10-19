using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client.Views;

public partial class MainWindow : Window
{
    // Add this variable to remember the start screen layout
    public static object? StartScreenContent;

    public MainWindow()
    {
        InitializeComponent();
        
        // Save the original UI right after it loads
        StartScreenContent = this.Content;
    }

    private void NavigateToLogin_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new LoginView();
    }

    private void NavigateToSignup_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new SignupView();
    }

    private void NavigateToHomePage_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new HomePageView();
    }
}