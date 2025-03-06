using System;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using expensesmanagerapp.Models;
using System.Windows;
using System.Transactions;
using expensesmanagerapp.UserControls;

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
                    string query = "SELECT amount, type, category FROM usertransactions WHERE user_id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserSession.Instance.UserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            decimal totalIncome = 0;
                            decimal totalOutcome = 0;
                            Dictionary<string, decimal> categoryOutcomes = new Dictionary<string, decimal>();

                            while (reader.Read())
                            {
                                decimal amount = Convert.ToDecimal(reader["amount"]);
                                string type = reader["type"].ToString().ToLower();
                                string category = reader["category"].ToString();

                                if (type == "income")
                                {
                                    totalIncome += amount;
                                }
                                else if (type == "outcome")
                                {
                                    totalOutcome += amount;
                                    if (categoryOutcomes.ContainsKey(category))
                                    {
                                        categoryOutcomes[category] += amount;
                                    }
                                    else
                                    {
                                        categoryOutcomes[category] = amount;
                                    }
                                }
                            }

                            UserSession.Instance.SetTransactions(totalIncome, totalOutcome);
                            UserSession.Instance.SetCategoryOutcomes(categoryOutcomes);
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
                        SELECT amount, type, description
                        FROM usertransactions
                        WHERE user_id = @UserId
                        ORDER BY date DESC
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
                                    Type = reader["type"].ToString(),
                                    Description = reader["description"].ToString()
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

        public List<RecentTransactions.Transaction> GetRecentTransactionsPaged(int userId, int pageNumber, int pageSize)
        {
            List<RecentTransactions.Transaction> transactions = new List<RecentTransactions.Transaction>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT id, amount, description, type, date
                        FROM usertransactions
                        WHERE user_id = @UserId
                        ORDER BY date DESC
                        LIMIT @PageSize OFFSET @Offset";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);
                        cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new RecentTransactions.Transaction
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Amount = Convert.ToDecimal(reader["amount"]),
                                    Description = reader["description"].ToString(),
                                    Type = reader["type"].ToString(),
                                    Date = Convert.ToDateTime(reader["date"])
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

        public bool UpdateTransaction(int transactionId, DateTime date, decimal amount, string type, string description, string category)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE usertransactions SET date = @Date, amount = @Amount, type = @Type, description = @Description, category = @Category WHERE id = @TransactionId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@Category", category);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        public bool DeleteTransaction(int transactionId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM usertransactions WHERE id = @TransactionId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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

        public Dictionary<string, decimal> CategoryOutcomes { get; private set; }

        public void SetCategoryOutcomes(Dictionary<string, decimal> categoryOutcomes)
        {
            CategoryOutcomes = categoryOutcomes;
        }

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

