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
            typeComboBox.SelectionChanged += TypeComboBox_SelectionChanged;
            LoadCategories("Income");
            LoadCategories("Outcome");
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (typeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedType = selectedItem.Content.ToString();
                LoadCategories(selectedType);
            }
        }

        private void LoadCategories(string type)
        {
            categoryComboBox.Items.Clear(); // Clear existing categories

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT category FROM transactioncategories WHERE type = @Type";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Type", type);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string category = reader["category"].ToString();
                                categoryComboBox.Items.Add(new ComboBoxItem { Content = category });
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

            //Select first Item if there is any.
            if (categoryComboBox.Items.Count > 0)
            {
                categoryComboBox.SelectedIndex = 0;
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

                                // Find and select the correct type
                                string typeFromDb = reader["type"].ToString();
                                foreach (ComboBoxItem item in typeComboBox.Items)
                                {
                                    if (item.Content.ToString() == typeFromDb)
                                    {
                                        typeComboBox.SelectedItem = item;
                                        break;
                                    }
                                }

                                // Find and select the correct category
                                string categoryFromDb = reader["category"].ToString();
                                foreach (ComboBoxItem item in categoryComboBox.Items)
                                {
                                    if (item.Content.ToString() == categoryFromDb)
                                    {
                                        categoryComboBox.SelectedItem = item;
                                        break;
                                    }
                                }

                                desctxbox.Text = reader["description"].ToString();
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
            string type = (typeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string description = desctxbox.Text;
            string category = (categoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
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
