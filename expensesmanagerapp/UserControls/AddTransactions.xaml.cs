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
using MySql.Data.MySqlClient;
using expensesmanagerapp.Models;

namespace expensesmanagerapp.UserControls
{
    /// <summary>
    /// Interaction logic for AddTransactions.xaml
    /// </summary>
    public partial class AddTransactions : UserControl
    {
        private readonly string connectionString = "server=localhost;database=exmanadb;uid=root;pwd=;";
        public AddTransactions()
        {
            InitializeComponent();
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

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            // Input validation
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
            string type = ((ComboBoxItem)typeComboBox.SelectedItem).Content.ToString();
            string description = desctxbox.Text;
            string category = ((ComboBoxItem)categoryComboBox.SelectedItem).Content.ToString();
            int userId = UserSession.Instance.UserId;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO usertransactions (user_id, date, amount, type, description, category) VALUES (@UserId, @Date, @Amount, @Type, @Description, @Category)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@Category", category);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Transaction added successfully.");
                        ClearFields(); // Clear input fields
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
            Dashboard dashboard = new Dashboard();
            if (Parent is Grid parentGrid)
            {
                parentGrid.Children.Clear();
                parentGrid.Children.Add(dashboard);
            }
            else if (Parent is Panel parentPanel)
            {
                parentPanel.Children.Clear();
                parentPanel.Children.Add(dashboard);
            }
            else
            {
                MessageBox.Show("Cannot navigate back. Parent container not supported.");
            }
        }

        private void ClearFields()
        {
            datePicker.SelectedDate = null;
            amounttxbox.Clear();
            desctxbox.Clear();
            typeComboBox.SelectedIndex = 0;
            categoryComboBox.SelectedIndex = 0;
        }
    }
}
