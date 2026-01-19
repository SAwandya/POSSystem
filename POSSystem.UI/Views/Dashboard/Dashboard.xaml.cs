using Microsoft.Extensions.DependencyInjection;
using POSSystem.UI.Views.Customer;
using POSSystem.UI.Views.GRN;
using POSSystem.UI.Views.inventory;
using POSSystem.UI.Views.Reports;
using POSSystem.UI.Views.Stock;
using System;
using System.Windows;
using System.Windows.Controls;

namespace POSSystem.UI.Views.Dashboard
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        // Sidebar Navigation Events
        private void DashboardMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Dashboard Menu clicked");
        }

        private void InventoryMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Inventory Menu clicked");
        }

        private void SalesMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Sales Menu clicked");
        }

        private void CustomersMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Customers Menu clicked");
        }

        private void AnalyticsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Analytics Menu clicked");
        }

        private void SettingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Settings Menu clicked");
        }

        // Quick Actions Events
        private void NewSaleButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("New Sale clicked");
        }

        private void StockButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get Inventory from DI container
                var app = (App)System.Windows.Application.Current;
                var inventory = app.ServiceProvider.GetRequiredService<Inventory>();
                inventory.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Inventory: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            var customerPage = new CustomerPage();
            customerPage.Show();
            ShowMessage("Customers clicked");
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Report = new ReportDashboards();
                Report.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Reports: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GRNButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var grnWindow = new GRNPage();
                grnWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening GRN: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PurchaseOrderButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Purchase Order clicked");
        }

        private void InvoiceHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Invoice History clicked");
        }

        private void QuickSaleButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Quick Sale clicked");
        }

        private void StockCheckButton_Click(object sender, RoutedEventArgs e)
        {
            StockPage stockpage = new StockPage();
            stockpage.Show();
        }

        private void ReturnsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Returns clicked");
        }

        private void CashDrawerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Cash Drawer clicked");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Settings clicked");
        }

        // Footer Events
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Help clicked");
        }

        private void DocsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Docs clicked");
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Refresh clicked");
        }

        private void ShowMessage(string message)
        {
            Console.WriteLine($"[Dashboard] {message}");
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}