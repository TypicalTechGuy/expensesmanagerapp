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

namespace expensesmanagerapp.UserControls
{
    /// <summary>
    /// Interaction logic for RecentTransactions.xaml
    /// </summary>
    public partial class RecentTransactions : UserControl
    {
        private int currentPage = 1;
        private int pageSize = 8;
        private List<Transaction> transactions;
        private dbConnect dbConnection;
        private UserSession userSession;
        public RecentTransactions()
        {
            InitializeComponent();
            userSession = UserSession.Instance;
            dbConnection = new dbConnect();
            LoadTransactions();
            DisplayTransactions();
        }
        private void LoadTransactions()
        {
            if (userSession != null && userSession.UserId != 0)
            {
                transactions = dbConnection.GetRecentTransactionsPaged(userSession.UserId, currentPage, pageSize);
            }
        }

        private void DisplayTransactions()
        {
            TransactionsPanel.Children.Clear(); // Clear existing transactions

            if (transactions != null && transactions.Count > 0)
            {
                foreach (var transaction in transactions)
                {
                    AddTransactionRow(transaction);
                }
            }
            else
            {
                Label noTransactionsLabel = new Label();
                noTransactionsLabel.Content = "No transactions found.";
                noTransactionsLabel.Foreground = System.Windows.Media.Brushes.White;
                TransactionsPanel.Children.Add(noTransactionsLabel);
            }

            UpdatePaginationButtons();
        }

        private void AddTransactionRow(Transaction transaction)
        {
            Grid transactionRow = new Grid();
            transactionRow.ColumnDefinitions.Add(new ColumnDefinition());
            transactionRow.ColumnDefinitions.Add(new ColumnDefinition());
            transactionRow.ColumnDefinitions.Add(new ColumnDefinition());
            transactionRow.ColumnDefinitions.Add(new ColumnDefinition());

            Label dateLabel = new Label();
            dateLabel.Content = transaction.Date.ToString("yyyy-MM-dd");
            dateLabel.Foreground = System.Windows.Media.Brushes.White;
            Grid.SetColumn(dateLabel, 0);
            transactionRow.Children.Add(dateLabel);

            Label amountLabel = new Label();
            amountLabel.Content = $"Rp {transaction.Amount:N0}";
            amountLabel.Foreground = System.Windows.Media.Brushes.White;
            Grid.SetColumn(amountLabel, 1);
            transactionRow.Children.Add(amountLabel);

            Label descriptionLabel = new Label();
            descriptionLabel.Content = transaction.Description;
            descriptionLabel.Foreground = System.Windows.Media.Brushes.White;
            Grid.SetColumn(descriptionLabel, 2);
            transactionRow.Children.Add(descriptionLabel);

            StackPanel buttonsPanel = new StackPanel();
            buttonsPanel.Orientation = Orientation.Horizontal;
            Grid.SetColumn(buttonsPanel, 3);
            transactionRow.Children.Add(buttonsPanel);

            Button editButton = new Button();
            editButton.Content = "Edit";
            editButton.Click += (sender, e) => EditTransaction(transaction.Id);
            buttonsPanel.Children.Add(editButton);

            Button deleteButton = new Button();
            deleteButton.Content = "Delete";
            deleteButton.Click += (sender, e) => DeleteTransaction(transaction.Id);
            buttonsPanel.Children.Add(deleteButton);

            TransactionsPanel.Children.Add(transactionRow);
        }

        private void EditTransaction(int transactionId)
        {
            EditTransactions editTransactions = new EditTransactions(transactionId);
            if (Parent is Grid parentGrid)
            {
                parentGrid.Children.Clear();
                parentGrid.Children.Add(editTransactions);
            }
        }

        private void DeleteTransaction(int transactionId)
        {
            if (MessageBox.Show("Are you sure you want to delete this transaction?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (dbConnection.DeleteTransaction(transactionId))
                {
                    MessageBox.Show("Transaction deleted successfully.");
                    LoadTransactions();
                    DisplayTransactions();
                }
                else
                {
                    MessageBox.Show("Failed to delete transaction.");
                }
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            LoadTransactions();
            DisplayTransactions();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadTransactions();
                DisplayTransactions();
            }
        }

        private void UpdatePaginationButtons()
        {
            PreviousPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled = transactions != null && transactions.Count == pageSize;
        }

        private void BackArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Dashboard dashboard = new Dashboard();
            if (Parent is Grid parentGrid)
            {
                parentGrid.Children.Clear();
                parentGrid.Children.Add(dashboard);
            }
        }

        public class Transaction
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
