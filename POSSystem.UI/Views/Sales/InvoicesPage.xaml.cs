using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSystem.UI.Views.Sales
{
    public partial class InvoicesPage : Window
    {
        private readonly ISalesService _salesService;
        private ObservableCollection<SaleDto> _allInvoices;
        private ObservableCollection<SaleDto> _filteredInvoices;

        public InvoicesPage()
        {
            InitializeComponent();

            // Get service from DI
            var app = (App)System.Windows.Application.Current;
            _salesService = app.ServiceProvider.GetRequiredService<ISalesService>();

            // Initialize collections
            _allInvoices = new ObservableCollection<SaleDto>();
            _filteredInvoices = new ObservableCollection<SaleDto>();

            dgInvoices.ItemsSource = _filteredInvoices;

            // Set default date range (last 30 days)
            dpStartDate.SelectedDate = DateTime.Now.AddDays(-30);
            dpEndDate.SelectedDate = DateTime.Now;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInvoicesAsync();
        }

        private async void LoadInvoicesAsync()
        {
            try
            {
                // Show loading
                LoadingPanel.Visibility = Visibility.Visible;

                // Get date range
                var startDate = dpStartDate.SelectedDate ?? DateTime.Now.AddDays(-30);
                var endDate = dpEndDate.SelectedDate ?? DateTime.Now;

                // Fetch invoices from database
                var invoices = await _salesService.GetSalesByDateRangeAsync(startDate, endDate.AddDays(1));

                _allInvoices.Clear();
                _filteredInvoices.Clear();

                foreach (var invoice in invoices.OrderByDescending(i => i.SaleDate))
                {
                    _allInvoices.Add(invoice);
                    _filteredInvoices.Add(invoice);
                }

                // Update UI
                UpdateSummary();
                UpdateRecordCount();

                // Hide loading
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Error loading invoices:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummary()
        {
            var totalInvoices = _allInvoices.Count;
            var totalSales = _allInvoices.Sum(i => i.GrandTotal);

            txtSummary.Text = $"Total Invoices: {totalInvoices} | Total Sales: Rs {totalSales:#,##0.00}";
        }

        private void UpdateRecordCount()
        {
            var count = _filteredInvoices.Count;
            var total = _allInvoices.Count;

            if (count == total)
            {
                txtFilteredCount.Text = $"Showing: {count} invoice{(count != 1 ? "s" : "")}";
            }
            else
            {
                txtFilteredCount.Text = $"Showing: {count} of {total} invoice{(total != 1 ? "s" : "")}";
            }

            txtRecordCount.Text = $"{count} invoice{(count != 1 ? "s" : "")} found";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Date filter changed - reload data
            if (IsLoaded)
            {
                LoadInvoicesAsync();
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInvoicesAsync();
        }

        private void ApplyFilters()
        {
            if (_allInvoices == null) return;

            var searchTerm = txtSearch.Text.ToLower().Trim();

            _filteredInvoices.Clear();

            foreach (var invoice in _allInvoices)
            {
                bool matches = true;

                // Search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    matches = invoice.InvoiceNumber.ToLower().Contains(searchTerm) ||
                             invoice.CustomerName.ToLower().Contains(searchTerm) ||
                             invoice.SaleId.ToString().Contains(searchTerm);
                }

                if (matches)
                {
                    _filteredInvoices.Add(invoice);
                }
            }

            UpdateRecordCount();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInvoicesAsync();
        }

        private void Invoice_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgInvoices.SelectedItem is SaleDto selectedInvoice)
            {
                ViewInvoiceDetails(selectedInvoice.SaleId);
            }
        }

        private void ViewInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int saleId)
            {
                ViewInvoiceDetails(saleId);
            }
        }

        private async void ViewInvoiceDetails(int saleId)
        {
            try
            {
                var invoice = await _salesService.GetSaleByIdAsync(saleId);

                if (invoice != null)
                {
                    // Create detailed view window
                    var detailsWindow = new Window
                    {
                        Title = $"Invoice Details - {invoice.InvoiceNumber}",
                        Width = 900,
                        Height = 700,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = this,
                        Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(15, 15, 26))
                    };

                    var scrollViewer = new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Padding = new Thickness(30)
                    };

                    var mainPanel = new StackPanel();

                    // Header
                    var headerText = new TextBlock
                    {
                        Text = $"?? INVOICE {invoice.InvoiceNumber}",
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 20)
                    };
                    mainPanel.Children.Add(headerText);

                    // Invoice Details Card
                    var detailsCard = new Border
                    {
                        Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(26, 26, 46)),
                        CornerRadius = new CornerRadius(12),
                        Padding = new Thickness(20),
                        Margin = new Thickness(0, 0, 0, 20)
                    };

                    var detailsGrid = new Grid();
                    
                    // Define columns
                    detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                    detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                    detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    
                    // Define rows
                    for (int i = 0; i < 5; i++)
                    {
                        detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    // Add details in proper grid positions
                    AddProperDetailRow(detailsGrid, 0, 0, "Invoice Date:", invoice.SaleDate.ToString("dd MMM yyyy"));
                    AddProperDetailRow(detailsGrid, 0, 2, "Invoice Time:", invoice.SaleDate.ToString("hh:mm tt"));
                    
                    AddProperDetailRow(detailsGrid, 1, 0, "Customer:", invoice.CustomerName ?? "Walk-in Customer");
                    AddProperDetailRow(detailsGrid, 1, 2, "Payment Status:", invoice.PaymentStatus);
                    
                    AddProperDetailRow(detailsGrid, 2, 0, "Subtotal:", $"Rs {invoice.SubTotal:#,##0.00}");
                    AddProperDetailRow(detailsGrid, 2, 2, "Tax Amount:", $"Rs {invoice.TaxAmount:#,##0.00}");
                    
                    AddProperDetailRow(detailsGrid, 3, 0, "Discount:", $"Rs {invoice.DiscountAmount:#,##0.00}");
                    AddProperDetailRow(detailsGrid, 3, 2, "Grand Total:", $"Rs {invoice.GrandTotal:#,##0.00}");

                    detailsCard.Child = detailsGrid;
                    mainPanel.Children.Add(detailsCard);

                    // Items Header
                    var itemsHeader = new TextBlock
                    {
                        Text = "ITEMS PURCHASED",
                        FontSize = 18,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 20, 0, 15)
                    };
                    mainPanel.Children.Add(itemsHeader);

                    // Items List
                    int itemNumber = 1;
                    foreach (var item in invoice.Items)
                    {
                        var itemPanel = new Border
                        {
                            Background = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(37, 37, 64)),
                            CornerRadius = new CornerRadius(8),
                            Padding = new Thickness(15),
                            Margin = new Thickness(0, 0, 0, 10)
                        };

                        var itemGrid = new Grid();
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });

                        var numberText = new TextBlock
                        {
                            Text = $"{itemNumber}.",
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumn(numberText, 0);

                        var nameText = new TextBlock
                        {
                            Text = item.ProductName,
                            FontSize = 14,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = System.Windows.Media.Brushes.White,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.Wrap
                        };
                        Grid.SetColumn(nameText, 1);

                        var qtyText = new TextBlock
                        {
                            Text = $"Qty: {item.Quantity}",
                            FontSize = 13,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        Grid.SetColumn(qtyText, 2);

                        var priceText = new TextBlock
                        {
                            Text = $"@ Rs {item.UnitPrice:#,##0.00}",
                            FontSize = 13,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        Grid.SetColumn(priceText, 3);

                        var totalText = new TextBlock
                        {
                            Text = $"Rs {item.TotalPrice:#,##0.00}",
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(0, 224, 150)),
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Right
                        };
                        Grid.SetColumn(totalText, 4);

                        itemGrid.Children.Add(numberText);
                        itemGrid.Children.Add(nameText);
                        itemGrid.Children.Add(qtyText);
                        itemGrid.Children.Add(priceText);
                        itemGrid.Children.Add(totalText);

                        itemPanel.Child = itemGrid;
                        mainPanel.Children.Add(itemPanel);
                        itemNumber++;
                    }

                    // Summary Section
                    var summaryBorder = new Border
                    {
                        Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(26, 26, 46)),
                        CornerRadius = new CornerRadius(12),
                        Padding = new Thickness(20),
                        Margin = new Thickness(0, 20, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Width = 400
                    };

                    var summaryStack = new StackPanel();

                    AddSummaryLine(summaryStack, "Total Items:", invoice.Items.Count.ToString(), false);
                    AddSummaryLine(summaryStack, "Subtotal:", $"Rs {invoice.SubTotal:#,##0.00}", false);
                    AddSummaryLine(summaryStack, "Discount:", $"Rs {invoice.DiscountAmount:#,##0.00}", false);
                    AddSummaryLine(summaryStack, "Tax:", $"Rs {invoice.TaxAmount:#,##0.00}", false);
                    
                    // Separator
                    var separator = new Border
                    {
                        Height = 1,
                        Background = System.Windows.Media.Brushes.DarkGray,
                        Margin = new Thickness(0, 10, 0, 10)
                    };
                    summaryStack.Children.Add(separator);
                    
                    AddSummaryLine(summaryStack, "GRAND TOTAL:", $"Rs {invoice.GrandTotal:#,##0.00}", true);

                    summaryBorder.Child = summaryStack;
                    mainPanel.Children.Add(summaryBorder);

                    // Close Button
                    var closeButton = new Button
                    {
                        Content = "? Close",
                        Width = 150,
                        Height = 50,
                        Margin = new Thickness(0, 30, 0, 0),
                        Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(139, 93, 255)),
                        Foreground = System.Windows.Media.Brushes.White,
                        BorderThickness = new Thickness(0),
                        FontSize = 16,
                        FontWeight = FontWeights.SemiBold,
                        Cursor = Cursors.Hand,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    closeButton.Click += (s, ev) => detailsWindow.Close();
                    mainPanel.Children.Add(closeButton);

                    scrollViewer.Content = mainPanel;
                    detailsWindow.Content = scrollViewer;
                    detailsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoice details: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProperDetailRow(Grid grid, int row, int columnStart, string label, string value)
        {
            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.LightGray,
                Margin = new Thickness(0, 8, 0, 8),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(labelText, row);
            Grid.SetColumn(labelText, columnStart);

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 8, 0, 8),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(valueText, row);
            Grid.SetColumn(valueText, columnStart + 1);

            grid.Children.Add(labelText);
            grid.Children.Add(valueText);
        }

        private void AddSummaryLine(StackPanel panel, string label, string value, bool isTotal)
        {
            var lineGrid = new Grid { Margin = new Thickness(0, 5, 0, 5) };
            lineGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            lineGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = isTotal ? 16 : 14,
                FontWeight = isTotal ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isTotal ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.LightGray
            };
            Grid.SetColumn(labelText, 0);

            var valueText = new TextBlock
            {
                Text = value,
                FontSize = isTotal ? 18 : 14,
                FontWeight = FontWeights.Bold,
                Foreground = isTotal ? 
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 224, 150)) :
                    System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(valueText, 1);

            lineGrid.Children.Add(labelText);
            lineGrid.Children.Add(valueText);

            panel.Children.Add(lineGrid);
        }

        private void AddDetailRow(Grid grid, int row, int column, string label, string value)
        {
            // This method is no longer used, but keeping for backward compatibility
            AddProperDetailRow(grid, row, column * 2, label, value);
        }

        private void PrintInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality will be implemented here.", "Print Invoice",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel/PDF functionality will be implemented here.", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportsWindow = new Reports.ReportDashboards();
                reportsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening reports: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newSalePage = new NewSalePage();
                newSalePage.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening new sale page: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
