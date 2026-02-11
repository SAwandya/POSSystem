using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.Services;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace POSSystem.UI.Views.Sales
{
    public partial class NewSaleTabs : Window
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private readonly IUnitOfWork _unitOfWork;

        private int _tabCounter = 1;

        public NewSaleTabs()
        {
            InitializeComponent();

            var app = (App)System.Windows.Application.Current;
            _productService = app.ServiceProvider.GetRequiredService<IProductService>();
            _salesService = app.ServiceProvider.GetRequiredService<ISalesService>();
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            Loaded += NewSaleTabs_Loaded;
        }

        private void NewSaleTabs_Loaded(object sender, RoutedEventArgs e)
        {
            // Create first invoice automatically
            AddNewInvoiceTab();
        }

        // ---------------- Tab Management ----------------

        private void AddNewInvoiceTab()
        {
            // Create invoice editor (UserControl)
            var invoiceEditor = new InvoiceEditor(_productService, _salesService, _unitOfWork);

            // Create tab
            var tab = new TabItem
            {
                Content = invoiceEditor
            };

            // Temporary header until invoice number loads
            tab.Header = $"Invoice {_tabCounter++}";

            // Once invoice number is generated, update tab header
            invoiceEditor.InvoiceNumberChanged += (s, invoiceNo) =>
            {
                tab.Header = invoiceNo;
            };

            // Close request from inside invoice editor (optional)
            invoiceEditor.RequestCloseTab += (s, args) =>
            {
                TryCloseTab(tab);
            };

            tabInvoices.Items.Add(tab);
            tabInvoices.SelectedItem = tab;
        }

        private void TabCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Find the parent TabItem
                var tab = FindParent<TabItem>(btn);
                if (tab != null)
                    TryCloseTab(tab);
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parent = child;

            while (parent != null)
            {
                if (parent is T typed)
                    return typed;

                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }

            return null;
        }


        private void TryCloseTab(TabItem tab)
        {
            if (tab?.Content is not InvoiceEditor editor)
                return;

            // If there are unsaved items, confirm
            if (editor.HasItems())
            {
                var result = MessageBox.Show(
                    "This invoice has items.\n\nClose this invoice tab anyway?",
                    "Confirm Close",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            tabInvoices.Items.Remove(tab);

            // If all tabs closed, create a fresh one automatically
            if (tabInvoices.Items.Count == 0)
                AddNewInvoiceTab();
        }

        private InvoiceEditor? GetCurrentInvoiceEditor()
        {
            if (tabInvoices.SelectedItem is not TabItem tab)
                return null;

            return tab.Content as InvoiceEditor;
        }

        // ---------------- Header Buttons ----------------

        private void NewInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewInvoiceTab();
        }

        private void SaveHoldButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = GetCurrentInvoiceEditor();
            if (editor == null) return;

            editor.SaveAndHold();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = GetCurrentInvoiceEditor();
            if (editor == null) return;

            editor.RefreshProducts();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dashboard = new POSSystem.UI.Views.Dashboard.Dashboard();
                dashboard.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dashboard not found.\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        // ---------------- Window Close Protection ----------------

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // If any tab has items, warn
                foreach (var item in tabInvoices.Items)
                {
                    if (item is TabItem tab && tab.Content is InvoiceEditor editor)
                    {
                        if (editor.HasItems())
                        {
                            var result = MessageBox.Show(
                                "You have invoices with items.\n\nExit anyway?",
                                "Confirm Exit",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);

                            if (result != MessageBoxResult.Yes)
                            {
                                e.Cancel = true;
                                return;
                            }

                            break;
                        }
                    }
                }
            }
            catch
            {
                // ignore safety errors
            }

            base.OnClosing(e);
        }
    }
}
