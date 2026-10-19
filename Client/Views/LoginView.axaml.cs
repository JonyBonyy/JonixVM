using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Client.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }
    
    private void NavigateToHomePage_Click(object? sender, RoutedEventArgs e)
    {
        this.Content = new HomePageView();
    }

    private async void SubmitLogin_Click(object? sender, RoutedEventArgs e)
    {
        string username = Username.Text ?? "";
        string password = Password.Text ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Please fill in both fields.");
            return;
        }

        try
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            
            string credentials = $"{username}:{password}";
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            byte[] payload = new byte[credentialsBytes.Length + 1];
            
            payload[0] = 1; // Change to 2 for the SignupView
            Array.Copy(credentialsBytes, 0, payload, 1, credentialsBytes.Length);
            
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(payload, 0, payload.Length);
            Console.WriteLine("Binary request sent to server! Waiting for response...");

            // --- Wait for the server's response ---
            byte[] responseBuffer = new byte[1];
            int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

            if (bytesRead > 0 && responseBuffer[0] == 1) // 1 means Success
            {
                Console.WriteLine("Server approved! Navigating to Home Page...");
                
                // Navigate to the HomePageView safely on the UI thread
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var window = TopLevel.GetTopLevel(this) as Window;
                    if (window != null)
                    {
                        window.Content = new HomePageView();
                    }
                });
            }
            else // 0 means Fail
            {
                Console.WriteLine("Server rejected the request. Invalid credentials or username taken.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
        }
    }
}