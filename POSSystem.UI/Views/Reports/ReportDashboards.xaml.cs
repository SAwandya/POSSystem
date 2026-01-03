using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSystem.UI.Views.Reports
{
    public partial class ReportDashboards : Window
    {
        public ReportDashboards()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set window to full screen
            this.WindowState = WindowState.Maximized;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow window dragging from title bar
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export functionality would be implemented here.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality would be implemented here.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Refreshing data...", "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewStockReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening Stock Report...", "Stock Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewCustomerReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening Customer Report...", "Customer Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewFinancialReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening Financial Report...", "Financial Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Help documentation would open here.", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Schedule report generation would open here.", "Schedule", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChartPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbChartPeriod.SelectedItem != null)
            {
                var selectedItem = cmbChartPeriod.SelectedItem as ComboBoxItem;
               
            }
        }

        private void ViewAllProducts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening all products report...", "All Products", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewAllTransactions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening all transactions report...", "All Transactions", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}