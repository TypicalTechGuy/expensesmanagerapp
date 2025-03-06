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
    /// Interaction logic for SummaryPage.xaml
    /// </summary>
    public partial class SummaryPage : UserControl
    {
        private readonly string connectionString = "server=localhost;database=exmanadb;uid=root;pwd=;";
        private UserSession userSession;
        public SummaryPage()
        {
            InitializeComponent();
            userSession = UserSession.Instance;
            InitializeComboBoxes();
            LoadSummaryData();
        }

        private void InitializeComboBoxes()
        {
            MonthComboBox.ItemsSource = new List<string> {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };
            MonthComboBox.SelectedIndex = DateTime.Now.Month - 1;

            int currentYear = DateTime.Now.Year;
            List<int> years = new List<int>();
            for (int i = currentYear - 10; i <= currentYear + 10; i++)
            {
                years.Add(i);
            }
            YearComboBox.ItemsSource = years;
            YearComboBox.SelectedItem = currentYear;
        }

        private void LoadSummaryData()
        {
            if (MonthComboBox.SelectedItem == null || YearComboBox.SelectedItem == null)
            {
                return;
            }

            try
            {
                if (userSession != null && userSession.UserId != 0)
                {
                    int month = MonthComboBox.SelectedIndex + 1;
                    int year = (int)YearComboBox.SelectedItem;

                    DateTime firstDayOfMonth = new DateTime(year, month, 1);
                    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"
                            SELECT 
                                SUM(CASE WHEN type = 'Income' THEN amount ELSE 0 END) AS TotalIncome,
                                SUM(CASE WHEN type = 'Outcome' THEN amount ELSE 0 END) AS TotalOutcome
                            FROM usertransactions
                            WHERE user_id = @UserId AND date BETWEEN @StartDate AND @EndDate";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userSession.UserId);
                            cmd.Parameters.AddWithValue("@StartDate", firstDayOfMonth);
                            cmd.Parameters.AddWithValue("@EndDate", lastDayOfMonth);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    decimal totalIncome = reader["TotalIncome"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalIncome"]);
                                    decimal totalOutcome = reader["TotalOutcome"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalOutcome"]);

                                    IncomeLabel.Content = $"Rp {totalIncome:N0}";
                                    OutcomeLabel.Content = $"Rp {totalOutcome:N0}";
                                    BalanceLabel.Content = $"Rp {totalIncome - totalOutcome:N0}";
                                }
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

        private void MonthYearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSummaryData();
        }
    }
}
