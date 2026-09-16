using Avalonia.Controls;
using Avalonia.Interactivity; // Make sure this is added at the top

namespace Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Add this new method
    private void NavigateToLogin_Click(object? sender, RoutedEventArgs e)
    {
        // This replaces the entire Welcome screen with your LoginView
        this.Content = new LoginView();
    }

    private void NavigateToSignin_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new SigninView();
    }
}