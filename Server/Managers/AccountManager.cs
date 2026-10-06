using System;
using System.Threading.Tasks;
using Npgsql;

namespace Server.Managers;

public class AccountManager
{
    public async Task<bool> RegisterUserAsync(string username, string password)
    {
        // Explicitly define the connection string using the 'postgres' superuser
        string connString = "Host=localhost;Port=5432;Username=postgres;Password=1Jonix2VM3;Database=jonixvm";
        
        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Insert into the users table using parameterized queries for security
            // Note: Later we will add a BCrypt hashing function here for the password
            string query = "INSERT INTO users (username, password_hash) VALUES (@username, @password)";
            
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database Error (Registration): {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> LoginUserAsync(string username, string password)
    {
        string connString = "Host=localhost;Port=5432;Username=postgres;Password=1Jonix2VM3;Database=jonixvm";
        
        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Count how many rows match both the username and the password
            string query = "SELECT COUNT(*) FROM users WHERE username = @username AND password_hash = @password";
            
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);

            // ExecuteScalar returns the first column of the first row (the count)
            long count = (long)await cmd.ExecuteScalarAsync();
            
            // If count is greater than 0, the user exists and the password is correct
            return count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database Error (Login): {ex.Message}");
            return false;
        }
    }
}