using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using expensesmanagerapp.Models;
using MySql.Data.MySqlClient;

namespace expensesmanagerapp.UserControls
{
    /// <summary>
    /// Interaction logic for EditTransactions.xaml
    /// </summary>
    public partial class EditTransactions : UserControl
    {
        private int transactionId;
        private readonly string connectionString = "server=localhost;database=exmanadb;uid=root;pwd=;";
        private UserSession userSession;
        public EditTransactions(int transactionId)
        {
            InitializeComponent();
            this.transactionId = transactionId;
            userSession = UserSession.Instance;
            LoadTransactionData();
            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {
            LoadTypes();
            LoadCategories();
        }

        private void LoadTypes()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT type FROM usertransactions WHERE user_id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userSession.UserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                typeComboBox.Items.Add(reader["type"].ToString());
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
        }

        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DISTINCT category FROM usertransactions WHERE user_id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userSession.UserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categoryComboBox.Items.Add(reader["category"].ToString());
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
        }

        private void LoadTransactionData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT date, amount, type, description, category FROM usertransactions WHERE id = @TransactionId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransactionId", transactionId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                datePicker.SelectedDate = Convert.ToDateTime(reader["date"]);
                                amounttxbox.Text = Convert.ToDecimal(reader["amount"]).ToString();
                                typeComboBox.SelectedItem = reader["type"].ToString();
                                desctxbox.Text = reader["description"].ToString();
                                categoryComboBox.SelectedItem = reader["category"].ToString();
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
        }

        private ComboBoxItem FindComboBoxItem(ComboBox comboBox, string content)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == content)
                {
                    return item;
                }
            }
            return null;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (datePicker.SelectedDate == null || string.IsNullOrEmpty(amounttxbox.Text) || typeComboBox.SelectedItem == null || string.IsNullOrEmpty(desctxbox.Text) || categoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(amounttxbox.Text, out decimal amount))
            {
                MessageBox.Show("Invalid amount format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime date = datePicker.SelectedDate.Value;
            string type = typeComboBox.SelectedItem.ToString(); // Corrected line
            string description = desctxbox.Text;
            string category = categoryComboBox.SelectedItem.ToString(); // Corrected line
            int userId = userSession.UserId;

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

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Transaction updated successfully.");
                        RecentTransactions recentTransactions = new RecentTransactions();
                        if (Parent is Grid parentGrid)
                        {
                            parentGrid.Children.Clear();
                            parentGrid.Children.Add(recentTransactions);
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


        private void BackArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecentTransactions recentTransactions = new RecentTransactions();
            if (Parent is Grid parentGrid)
            {
                parentGrid.Children.Clear();
                parentGrid.Children.Add(recentTransactions);
            }
        }
    }
}
