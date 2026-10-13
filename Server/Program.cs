using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Managers;

namespace Server;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Server");

        // 1. Initialize and test the database connection
        DatabaseManager dbManager = new DatabaseManager();
        bool isDbConnected = await dbManager.TestConnectionAsync();

        if (!isDbConnected)
        {
            Console.WriteLine("Fatal Error: Shutting down server because database is unreachable.");
            return; // This completely stops the program
        }

        // 2. Start the TCP Listener on port 5000
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
        listener.Start();
        Console.WriteLine("Server is actively listening on port 5000...");

        // 3. Keep the server awake to accept incoming connections
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("An Avalonia client has connected to the server!");
            
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024]; 
            
            // Wait for the client to send the binary data
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            
            if (bytesRead > 0)
            {
                // Read the first byte to determine which action the client is requesting
                byte commandType = buffer[0];
                Console.WriteLine($"Received binary data! Command ID: {commandType}, Total Bytes: {bytesRead}");
                
                // Opcode 1 = Log In
                if (commandType == 1)
                {
                    string credentials = Encoding.UTF8.GetString(buffer, 1, bytesRead - 1);
                    string[] parts = credentials.Split(':'); 
                    
                    if (parts.Length == 2)
                    {
                        string username = parts[0];
                        string password = parts[1];

                        AccountManager accountManager = new AccountManager();
                        bool isValid = await accountManager.LoginUserAsync(username, password);
                        
                        if (isValid)
                        {
                            Console.WriteLine($"Login Success! User '{username}' is now connected.");
                            // Later: Send a success byte back to the client
                        }
                        else
                        {
                            Console.WriteLine($"Login Failed! Incorrect username or password for '{username}'.");
                            // Later: Send a failure byte back to the client
                        }
                    }
                }
                // Opcode 2 = Sign In / Register
                else if (commandType == 2)
                {
                    // Extract everything after the first byte (the Opcode)
                    string credentials = Encoding.UTF8.GetString(buffer, 1, bytesRead - 1);
                    string[] parts = credentials.Split(':'); // parts[0] is username, parts[1] is password
                    
                    if (parts.Length == 2)
                    {
                        string username = parts[0];
                        string password = parts[1];

                        AccountManager accountManager = new AccountManager();
                        bool success = await accountManager.RegisterUserAsync(username, password);
                        
                        if (success)
                        {
                            Console.WriteLine($"Success! User '{username}' has been saved to the database.");
                        }
                    }
                }
            }
            
            client.Close();
        }
    }
}