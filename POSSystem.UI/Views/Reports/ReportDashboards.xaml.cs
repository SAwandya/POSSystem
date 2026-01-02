using System.Windows;
using System.Windows.Controls;

namespace POSSystem.UI.Views.Reports
{
    public partial class ReportDashboards : Window
    {
        public ReportDashboards()
        {
            InitializeComponent();
        }

        private void ViewAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View All Transactions clicked");
        }

        private void ViewFinancialReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View Financial Report clicked");
        }

        private void ViewAllProducts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View All Products clicked");
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Schedule clicked");
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Help clicked");
        }

        private void ChartPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // You can handle chart updates here
        }
    }
}
