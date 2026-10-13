using Npgsql;
using System;
using System.Threading.Tasks;

namespace Server.Managers;

public class DatabaseManager
{
    // Update the password to match the actual PostgreSQL setup
    private readonly string _connectionString = "Host=127.0.0.1;Username=postgres;Password=YOUR_PASSWORD;Database=jonixvm";

    // A simple method to verify the server can talk to the database
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            Console.WriteLine("Successfully connected to the PostgreSQL database!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }
}