using System;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using expensesmanagerapp.Models;
using System.Windows;
using System.Transactions;

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

        public bool ValidateUser(string email, string password)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT id, username, balance, income, outcome, password_hash 
                        FROM users WHERE email = @Email";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["id"]);
                                string username = reader["username"].ToString();
                                decimal balance = Convert.ToDecimal(reader["balance"]);
                                decimal income = Convert.ToDecimal(reader["income"]);
                                decimal outcome = Convert.ToDecimal(reader["outcome"]);
                                string storedHash = reader["password_hash"].ToString();

                                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                                {
                                    UserSession.Instance.SetUser(userId, username, balance, income, outcome);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        public void LoadUserTransactions()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT type, SUM(amount) as total FROM usertransactions WHERE user_id = @UserId GROUP BY type";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserSession.Instance.UserId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            decimal totalIncome = 0, totalOutcome = 0;

                            while (reader.Read())
                            {
                                string type = reader["type"].ToString();
                                decimal amount = Convert.ToDecimal(reader["total"]);

                                if (type == "Income") totalIncome = amount;
                                else if (type == "Outcome") totalOutcome = amount;
                            }

                            UserSession.Instance.SetTransactions(totalIncome, totalOutcome);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading transactions: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<Transaction> GetRecentTransactions(int userId)
        {
            List<Transaction> transactions = new List<Transaction>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT amount, description, type
                        FROM usertransactions
                        WHERE user_id = @UserId
                        ORDER BY created_at DESC
                        LIMIT 2";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    Amount = Convert.ToDecimal(reader["amount"]),
                                    Description = reader["description"].ToString(),
                                    Type = reader["type"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return transactions;
        }

        public class Transaction
        {
            public decimal Amount { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
        }
    }


    public class UserSession
    {
        private static UserSession instance;
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public decimal Balance { get; private set; }
        public decimal Income { get; private set; }
        public decimal Outcome { get; private set; }

        private UserSession() { }

        public static UserSession Instance
        {
            get
            {
                if (instance == null)
                    instance = new UserSession();
                return instance;
            }
        }

        public void SetUser(int userId, string username, decimal balance, decimal income, decimal outcome)
        {
            UserId = userId;
            Username = username;
            Balance = balance;
            Income = income;
            Outcome = outcome;
        }

        public void SetTransactions(decimal income, decimal outcome)
        {
            Income = income;
            Outcome = outcome;
            Balance = income - outcome;
        }

        public void ClearSession()
        {
            instance = null;
        }

        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}

