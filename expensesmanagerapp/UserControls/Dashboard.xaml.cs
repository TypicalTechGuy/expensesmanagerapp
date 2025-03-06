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
using LiveCharts;
using LiveCharts.Wpf;
using expensesmanagerapp.Models;

namespace expensesmanagerapp.UserControls
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private UserSession userSession;
        private dbConnect dbConnection;

        public ChartValues<double> BillsValue { get; set; }
        public ChartValues<double> FoodValue { get; set; }
        public ChartValues<double> EducationValue { get; set; }
        public ChartValues<double> ShoppingValue { get; set; }
        public ChartValues<double> EntertainmentValue { get; set; }
        public ChartValues<double> MiscellaneousValue { get; set; }
        public Dashboard()
        {
            InitializeComponent();
            userSession = UserSession.Instance;
            dbConnection = new dbConnect();
            LoadUserData();
            UpdateUI();
            LoadRecentTransactions();
            LoadChartData();
            DataContext = this;
        }

        private void LoadUserData()
        {
            if (userSession.UserId != 0)
            {
                dbConnection.LoadUserTransactions();
            }
        }

        private void UpdateUI()
        {
            if (userSession != null && userSession.UserId != 0)
            {
                greetingLabel.Content = $"{GetTimeGreeting()}";
                greetingLabel_2.Content = $"{userSession.Username}";
                balanceLabel.Content = $"Balance: Rp {userSession.Balance:N0}";
                moneyIncome.Content = $"Rp {userSession.Income:N0}";
                moneySpending.Content = $"Rp {userSession.Outcome:N0}";
            }
            else
            {
                greetingLabel.Content = "Welcome, Guest";
                balanceLabel.Content = "Balance: Rp 0";
                moneyIncome.Content = "Rp 0";
                moneySpending.Content = "Rp 0";
            }
        }

        private void LoadChartData()
        {
            BillsValue = new ChartValues<double> { 0 };
            FoodValue = new ChartValues<double> { 0 };
            EducationValue = new ChartValues<double> { 0 };
            ShoppingValue = new ChartValues<double> { 0 };
            EntertainmentValue = new ChartValues<double> { 0 };
            MiscellaneousValue = new ChartValues<double> { 0 };

            if (userSession != null && userSession.CategoryOutcomes != null)
            {
                if (userSession.CategoryOutcomes.ContainsKey("Bills"))
                    BillsValue[0] = (double)userSession.CategoryOutcomes["Bills"];
                if (userSession.CategoryOutcomes.ContainsKey("Food"))
                    FoodValue[0] = (double)userSession.CategoryOutcomes["Food"];
                if (userSession.CategoryOutcomes.ContainsKey("Education"))
                    EducationValue[0] = (double)userSession.CategoryOutcomes["Education"];
                if (userSession.CategoryOutcomes.ContainsKey("Shopping"))
                    ShoppingValue[0] = (double)userSession.CategoryOutcomes["Shopping"];
                if (userSession.CategoryOutcomes.ContainsKey("Entertainment"))
                    EntertainmentValue[0] = (double)userSession.CategoryOutcomes["Entertainment"];
                if (userSession.CategoryOutcomes.ContainsKey("Misc."))
                    MiscellaneousValue[0] = (double)userSession.CategoryOutcomes["Misc."];
            }
        }

        private void addTransaction_Click(object sender, RoutedEventArgs e)
        {
            AddTransactions AddTransactionsPage = new AddTransactions();
            DashboardPage.Children.Clear();
            DashboardPage.Children.Add(AddTransactionsPage);

        }

        private void editTransaction_Click(object sender, RoutedEventArgs e)
        {
            EditTransactions EditTransactionsPage = new EditTransactions(-1);
            DashboardPage.Children.Clear();
            DashboardPage.Children.Add(EditTransactionsPage);
        }

        private void ExpenseSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            SummaryPage summaryPage = new SummaryPage();
            DashboardPage.Children.Clear();
            DashboardPage.Children.Add(summaryPage);
        }

        private void SeeAllLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecentTransactions recentTransactionsPage = new RecentTransactions();
            DashboardPage.Children.Clear();
            DashboardPage.Children.Add(recentTransactionsPage);
        }

        private string GetTimeGreeting()
        {
            int hour = DateTime.Now.Hour;
            if (hour < 12) return "Good Morning";
            else if (hour < 18) return "Good Afternoon";
            else return "Good Evening";
        }

        private void LoadRecentTransactions()
        {
            if (userSession != null && userSession.UserId != 0)
            {
                List<dbConnect.Transaction> recentTransactions = dbConnection.GetRecentTransactions(userSession.UserId);
                if (recentTransactions.Count > 0)
                {
                    if (recentTransactions.Count > 0)
                    {
                        string transaction1Color = recentTransactions[0].Type == "income" ? "#FF00FF22" : "Red";
                        Label transaction1Label = FindName("recentTransaction1") as Label;
                        if (transaction1Label != null)
                        {
                            transaction1Label.Content = $"Rp {recentTransactions[0].Amount:N0}\n{recentTransactions[0].Description}";
                        }
                    }
                    if (recentTransactions.Count > 1)
                    {
                        string transaction2Color = recentTransactions[1].Type == "income" ? "#FF00FF22" : "Red";
                        Label transaction2Label = FindName("recentTransaction2") as Label;
                        if (transaction2Label != null)
                        {
                            transaction2Label.Content = $"Rp {recentTransactions[1].Amount:N0}\n{recentTransactions[1].Description}";
                        }
                    }
                }
            }
        }
    }
}
