using System;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using expensesmanagerapp.Models;
using System.Windows;

namespace expensesmanagerapp.Models
{
    public class dbConnect
    {
        private readonly string connectionString = "server=localhost;database=exmanadb;uid=root;pwd=;";

        // Hash Password using SHA-256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Method to Register a User
        public bool RegisterUser(User user)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Users (Username, Email, Password_Hash) VALUES (@Username, @Email, @PasswordHash)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("User registered successfully!");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("No rows affected. Registration failed.");
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine("MySQL Error: " + sqlEx.Message);
                MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Error: " + ex.Message);
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
