using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Client.Views;

public partial class SignupView : UserControl
{
    public SignupView()
    {
        InitializeComponent();
    }

    private async void SubmitSignin_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Read the input data from the UI
        string username = Username.Text ?? "";
        string password = Password.Text ?? "";
        string confirm = ConfirmPassword.Text ?? "";

        // 2. Basic validation to check if passwords match
        if (password != confirm)
        {
            Console.WriteLine("Passwords do not match");
            return; 
        }

        // 3. Send the data to the server
        await SendRegisterRequestAsync(username, password);
    }

    private async Task SendRegisterRequestAsync(string username, string password)
    {
        try
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            
            // Format the credentials with a colon separator so the server can parse them
            string credentials = $"{username}:{password}";
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            
            // Create a new byte array that is 1 byte larger to fit the Opcode at the beginning
            byte[] payload = new byte[credentialsBytes.Length + 1];
            
            // The first byte (index 0) is our Opcode. 2 = Sign In / Register
            payload[0] = 2;
            
            // Copy the rest of the data into the array, immediately after the Opcode
            Array.Copy(credentialsBytes, 0, payload, 1, credentialsBytes.Length);
            
            // Send to the server
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(payload, 0, payload.Length);
            
            Console.WriteLine("Binary Sign-In request sent to server!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
        }
    }
}