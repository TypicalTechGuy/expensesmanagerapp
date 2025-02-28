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

namespace expensesmanagerapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void usertxbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Login Success");
            Dashboard dashboardPage = new Dashboard();
            MainWindows.Children.Clear();
            MainWindows.Children.Add(dashboardPage);
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Redirecting to the Registration Page...");
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Redirecting to Password Recovery...");
        }
    }
}