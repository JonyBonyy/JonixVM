using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    //Switches to Log in screen
    private void NavigateToLogin_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new LoginView();
    }

    //Switches to Sign in screen
    private void NavigateToSignup_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new SignupView();
    }
}