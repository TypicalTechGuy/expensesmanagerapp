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

namespace expensesmanagerapp.UserControls
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public ChartValues<double> BillsValue { get; set; }
        public ChartValues<double> EducationValue { get; set; }
        public ChartValues<double> ShoppingValue { get; set; }
        public ChartValues<double> EntertainmentValue { get; set; }
        public ChartValues<double> MiscellaneousValue { get; set; }
        public Dashboard()
        {
            InitializeComponent();
            BillsValue = new ChartValues<double> { 500000 };
            EducationValue = new ChartValues<double> { 100000 };
            ShoppingValue = new ChartValues<double> { 0 };
            EntertainmentValue = new ChartValues<double> { 400000 };
            MiscellaneousValue = new ChartValues<double> { 0 };
            DataContext = this;
        }
    }
}
