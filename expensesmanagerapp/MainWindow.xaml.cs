using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using expensesmanagerapp.UserControls;
using expensesmanagerapp.Models;
using BCrypt.Net;

namespace expensesmanagerapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private dbConnect database = new dbConnect();
        public MainWindow()
        {
            InitializeComponent();
        }


        private void usertxbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = usertxbox.Text.Trim();
            string password = pwtxbox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool isValidUser = database.ValidateUser(email, password);
            if (isValidUser) 
            {
                MessageBox.Show("Login Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Dashboard dashboardPage = new Dashboard();
                MainWindows.Children.Clear();
                MainWindows.Children.Add(dashboardPage);
            }
            else
            {
                MessageBox.Show("Invalid Email or Password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } 
        

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            Register registerPage = new Register();
            MainWindows.Children.Clear();
            MainWindows.Children.Add(registerPage);
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            ForgotPassword forgotPass = new ForgotPassword();
            MainWindows.Children.Clear();
            MainWindows.Children.Add(forgotPass);
        }
    }
}