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
            
            // Opcode 1 = Log In
            payload[0] = 1;
            Array.Copy(credentialsBytes, 0, payload, 1, credentialsBytes.Length);
            
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(payload, 0, payload.Length);
            
            Console.WriteLine("Binary Log-In request sent to server!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
        }
    }
}