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
            // Create the hash from the plain text password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Insert into the users table using parameterized queries for security
            // Note: Later we will add a BCrypt hashing function here for the password
            string query = "INSERT INTO users (username, password_hash) VALUES (@username, @password)";
            
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", hashedPassword);

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

            // Fetch only the password hash for this username
            string query = "SELECT password_hash FROM users WHERE username = @username";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("username", username);

            // Read the result
            await using var reader = await cmd.ExecuteReaderAsync();
            
            // If we found the user in the database
            if (await reader.ReadAsync())
            {
                string storedHash = reader.GetString(0);
                
                // Compare the plain text password from the user with the stored hash
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, storedHash);
                return isPasswordCorrect;
            }
            
            // User not found
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database Error (Login): {ex.Message}");
            return false;
        }
    }
}