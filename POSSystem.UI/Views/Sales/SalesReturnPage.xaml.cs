using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace POSSystem.UI.Views.Sales
{
    public partial class SalesReturnPage : Window
    {
        private readonly ISalesService _salesService;
        private readonly IUnitOfWork _unitOfWork;
        private ObservableCollection<SaleDto> _invoices;

        public SalesReturnPage()
        {
            InitializeComponent();

            // Get services from DI
            var app = (App)System.Windows.Application.Current;
            _salesService = app.ServiceProvider.GetRequiredService<ISalesService>();
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Initialize collections
            _invoices = new ObservableCollection<SaleDto>();
            dgInvoices.ItemsSource = _invoices;

            // Set default date range (last 30 days)
            dpInvoiceStartDate.SelectedDate = DateTime.Now.AddDays(-30);
            dpInvoiceEndDate.SelectedDate = DateTime.Now;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto-load recent invoices
            SearchInvoices_Click(null, null);
        }

        private async void SearchInvoices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var startDate = dpInvoiceStartDate.SelectedDate ?? DateTime.Now.AddDays(-30);
                var endDate = dpInvoiceEndDate.SelectedDate ?? DateTime.Now;

                var invoices = await _salesService.GetSalesByDateRangeAsync(startDate, endDate.AddDays(1));

                _invoices.Clear();

                var searchTerm = txtInvoiceSearch?.Text?.ToLower().Trim() ?? string.Empty;

                foreach (var invoice in invoices.OrderByDescending(i => i.SaleDate))
                {
                    // Filter by search term if provided
                    if (string.IsNullOrWhiteSpace(searchTerm) ||
                        invoice.InvoiceNumber.ToLower().Contains(searchTerm) ||
                        invoice.CustomerName.ToLower().Contains(searchTerm) ||
                        invoice.SaleId.ToString().Contains(searchTerm))
                    {
                        _invoices.Add(invoice);
                    }
                }

                txtInvoiceCount.Text = $"({_invoices.Count} found)";

                if (_invoices.Count == 0)
                {
                    MessageBox.Show("No invoices found matching your search criteria.", "No Results",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching invoices:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InvoiceSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchInvoices_Click(sender, e);
            }
        }

        // Handle both double-click and single-click selection
        private void Invoice_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgInvoices.SelectedItem is SaleDto invoice)
            {
                OpenReturnDialog(invoice);
            }
        }

        // New handler for single-click selection
        private void Invoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is SaleDto invoice)
            {
                // Open dialog when row is clicked
                OpenReturnDialog(invoice);
                
                // Clear selection to allow clicking the same invoice again
                dgInvoices.SelectedItem = null;
            }
        }

        private void OpenReturnDialog(SaleDto invoice)
        {
            var dialog = new ReturnProcessDialog(invoice, _unitOfWork);
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                // Refresh invoice list after successful return
                SearchInvoices_Click(null, null);
                
                MessageBox.Show(
                    $"? RETURN PROCESSED SUCCESSFULLY!\n\n" +
                    $"Return has been completed and saved to database.\n" +
                    $"Inventory has been updated.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // ViewModel for Return Items
    public class ReturnItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private decimal _returnQuantity;

        public int SaleItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AlreadyReturned { get; set; }
        public decimal MaxReturnableQty { get; set; }
        public decimal UnitPrice { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public decimal ReturnQuantity
        {
            get => _returnQuantity;
            set
            {
                if (_returnQuantity != value)
                {
                    _returnQuantity = value;
                    OnPropertyChanged(nameof(ReturnQuantity));
                    OnPropertyChanged(nameof(RefundAmount));
                }
            }
        }

        public decimal RefundAmount => ReturnQuantity * UnitPrice;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Return Process Dialog
    public class ReturnProcessDialog : Window
    {
        private readonly SaleDto _invoice;
        private readonly IUnitOfWork _unitOfWork;
        private ObservableCollection<ReturnItemViewModel> _returnItems;
        private DataGrid dgItems;
        private ComboBox cmbReason;
        private ComboBox cmbCondition;
        private TextBlock txtItemCount;
        private TextBlock txtRefundAmount;

        public ReturnProcessDialog(SaleDto invoice, IUnitOfWork unitOfWork)
        {
            _invoice = invoice;
            _unitOfWork = unitOfWork;
            _returnItems = new ObservableCollection<ReturnItemViewModel>();

            InitializeDialog();
            LoadInvoiceItems();
        }

        private void InitializeDialog()
        {
            Title = $"Process Return - {_invoice.InvoiceNumber}";
            Width = 900;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(15, 15, 26));
            ResizeMode = ResizeMode.NoResize;

            var mainGrid = new Grid { Margin = new Thickness(30) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var header = new TextBlock
            {
                Text = $"?? RETURN PROCESSING",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(header, 0);
            mainGrid.Children.Add(header);

            // Invoice Info
            var invoiceInfo = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 46)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var infoStack = new StackPanel();
            infoStack.Children.Add(new TextBlock
            {
                Text = $"Invoice: {_invoice.InvoiceNumber} | Customer: {_invoice.CustomerName} | Date: {_invoice.SaleDate:dd MMM yyyy}",
                FontSize = 14,
                Foreground = Brushes.LightGray
            });
            infoStack.Children.Add(new TextBlock
            {
                Text = $"Original Total: Rs {_invoice.GrandTotal:#,##0.00}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 150)),
                Margin = new Thickness(0, 5, 0, 0)
            });

            invoiceInfo.Child = infoStack;
            Grid.SetRow(invoiceInfo, 1);
            mainGrid.Children.Add(invoiceInfo);

            // Items Grid
            var itemsLabel = new TextBlock
            {
                Text = "? Step 1: Select items to return",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            
            dgItems = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                CanUserResizeRows = false,
                IsReadOnly = false,
                SelectionMode = DataGridSelectionMode.Single,
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 46)),
                Foreground = Brushes.White,
                RowBackground = new SolidColorBrush(Color.FromRgb(26, 26, 46)),
                AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(37, 37, 64)),
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                RowHeight = 45,
                ItemsSource = _returnItems
            };

            // Create header style with black background
            var headerStyle = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BackgroundProperty, Brushes.Black));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.ForegroundProperty, Brushes.White));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(58, 58, 94))));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.BorderThicknessProperty, new Thickness(0, 0, 0, 2)));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.HeightProperty, 50.0));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.PaddingProperty, new Thickness(15, 0, 15, 0)));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontWeightProperty, FontWeights.Bold));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Primitives.DataGridColumnHeader.FontSizeProperty, 13.0));
            dgItems.ColumnHeaderStyle = headerStyle;

            // Columns
            dgItems.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Select",
                Width = new DataGridLength(60),
                CellTemplate = CreateCheckBoxTemplate()
            });

            dgItems.Columns.Add(new DataGridTextColumn
            {
                Header = "Product",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                Binding = new System.Windows.Data.Binding("ProductName"),
                IsReadOnly = true
            });

            dgItems.Columns.Add(new DataGridTextColumn
            {
                Header = "Sold Qty",
                Width = new DataGridLength(80),
                Binding = new System.Windows.Data.Binding("Quantity"),
                IsReadOnly = true
            });

            dgItems.Columns.Add(new DataGridTextColumn
            {
                Header = "Unit Price",
                Width = new DataGridLength(100),
                Binding = new System.Windows.Data.Binding("UnitPrice") { StringFormat = "Rs {0:#,##0.00}" },
                IsReadOnly = true
            });

            dgItems.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Return Qty",
                Width = new DataGridLength(100),
                CellTemplate = CreateQuantityTemplate()
            });

            var itemsPanel = new StackPanel();
            itemsPanel.Children.Add(itemsLabel);
            itemsPanel.Children.Add(dgItems);
            Grid.SetRow(itemsPanel, 2);
            mainGrid.Children.Add(itemsPanel);

            // Return Details
            var detailsPanel = new StackPanel { Margin = new Thickness(0, 20, 0, 0) };
            
            var detailsLabel = new TextBlock
            {
                Text = "? Step 2: Provide return details",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            detailsPanel.Children.Add(detailsLabel);

            var detailsGrid = new Grid();
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var reasonStack = new StackPanel();
            reasonStack.Children.Add(new TextBlock { Text = "Return Reason:", FontSize = 12, Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 5) });
            cmbReason = new ComboBox
            {
                Height = 45,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94))
            };
            
            // Style for dropdown items
            var itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, Brushes.Black));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.White));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(10, 8, 10, 8)));
            
            var hoverTrigger = new Trigger { Property = ComboBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(58, 58, 94))));
            itemStyle.Triggers.Add(hoverTrigger);
            
            var highlightTrigger = new Trigger { Property = ComboBoxItem.IsHighlightedProperty, Value = true };
            highlightTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, new SolidColorBrush(Color.FromRgb(58, 58, 94))));
            itemStyle.Triggers.Add(highlightTrigger);
            
            cmbReason.ItemContainerStyle = itemStyle;
            cmbReason.Items.Add(new ComboBoxItem { Content = "Defective Product", IsSelected = true });
            cmbReason.Items.Add(new ComboBoxItem { Content = "Wrong Item Received" });
            cmbReason.Items.Add(new ComboBoxItem { Content = "Customer Changed Mind" });
            cmbReason.Items.Add(new ComboBoxItem { Content = "Damaged in Transit" });
            cmbReason.Items.Add(new ComboBoxItem { Content = "Other" });
            reasonStack.Children.Add(cmbReason);
            Grid.SetColumn(reasonStack, 0);

            var conditionStack = new StackPanel();
            conditionStack.Children.Add(new TextBlock { Text = "Item Condition:", FontSize = 12, Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 5) });
            cmbCondition = new ComboBox
            {
                Height = 45,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                ItemContainerStyle = itemStyle
            };
            cmbCondition.Items.Add(new ComboBoxItem { Content = "Good (Resellable)", IsSelected = true });
            cmbCondition.Items.Add(new ComboBoxItem { Content = "Damaged (Not Resellable)" });
            conditionStack.Children.Add(cmbCondition);
            Grid.SetColumn(conditionStack, 2);

            detailsGrid.Children.Add(reasonStack);
            detailsGrid.Children.Add(conditionStack);
            detailsPanel.Children.Add(detailsGrid);
            Grid.SetRow(detailsPanel, 3);
            mainGrid.Children.Add(detailsPanel);

            // Summary
            var summaryBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),  // Black background
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 20, 0, 0)
            };

            var summaryGrid = new Grid();
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            summaryGrid.RowDefinitions.Add(new RowDefinition());
            summaryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            summaryGrid.RowDefinitions.Add(new RowDefinition());

            summaryGrid.Children.Add(new TextBlock { Text = "Items to Return:", FontSize = 14, Foreground = Brushes.LightGray });
            txtItemCount = new TextBlock { Text = "0", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(txtItemCount, 1);
            summaryGrid.Children.Add(txtItemCount);

            var refundLabel = new TextBlock { Text = "Refund Amount:", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
            Grid.SetRow(refundLabel, 2);
            summaryGrid.Children.Add(refundLabel);

            txtRefundAmount = new TextBlock { Text = "Rs 0.00", FontSize = 18, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 150)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetRow(txtRefundAmount, 2);
            Grid.SetColumn(txtRefundAmount, 1);
            summaryGrid.Children.Add(txtRefundAmount);

            summaryBorder.Child = summaryGrid;
            Grid.SetRow(summaryBorder, 4);
            mainGrid.Children.Add(summaryBorder);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var processButton = new Button
            {
                Content = "? Process Return",
                Width = 160,
                Height = 50,
                Background = new SolidColorBrush(Color.FromRgb(139, 93, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            processButton.Click += ProcessReturn_Click;

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 120,
                Height = 50,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                FontSize = 14,
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(processButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 5);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }

        private DataTemplate CreateCheckBoxTemplate()
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(CheckBox.WidthProperty, 24.0);
            factory.SetValue(CheckBox.HeightProperty, 24.0);
            factory.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding("IsSelected") { Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });
            factory.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(CheckBox_Changed));
            factory.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(CheckBox_Changed));
            template.VisualTree = factory;
            return template;
        }

        private DataTemplate CreateQuantityTemplate()
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBox));
            factory.SetValue(TextBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(TextBox.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0, 0, 0)));  // Black background
            factory.SetValue(TextBox.ForegroundProperty, Brushes.White);
            factory.SetValue(TextBox.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(58, 58, 94)));
            factory.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("ReturnQuantity") { Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });
            factory.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(Quantity_TextChanged));
            // Auto-tick when user enters quantity
            factory.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler(Quantity_LostFocus));
            template.VisualTree = factory;
            return template;
        }

        private void LoadInvoiceItems()
        {
            if (_invoice.Items == null || !_invoice.Items.Any())
            {
                MessageBox.Show("This invoice has no items to return.", "No Items",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DialogResult = false;
                return;
            }

            foreach (var item in _invoice.Items)
            {
                var returnItem = new ReturnItemViewModel
                {
                    SaleItemId = item.ItemId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName ?? "Unknown Product",
                    Quantity = item.Quantity,
                    AlreadyReturned = 0,
                    MaxReturnableQty = item.Quantity,
                    ReturnQuantity = 0,
                    UnitPrice = item.UnitPrice,
                    IsSelected = false
                };

                _returnItems.Add(returnItem);
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is ReturnItemViewModel item)
            {
                if (item.IsSelected && item.ReturnQuantity == 0)
                {
                    item.ReturnQuantity = item.MaxReturnableQty;
                }
                UpdateSummary();
            }
        }

        private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ReturnItemViewModel item)
            {
                // Auto-tick checkbox when quantity is entered
                if (decimal.TryParse(textBox.Text, out decimal qty) && qty > 0)
                {
                    if (!item.IsSelected)
                    {
                        item.IsSelected = true;
                    }
                }
                else if (qty == 0)
                {
                    // Uncheck if quantity is set to 0
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                }
            }
            
            UpdateSummary();
        }

        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ReturnItemViewModel item)
            {
                // Validate and auto-correct on focus lost
                if (decimal.TryParse(textBox.Text, out decimal qty))
                {
                    if (qty > item.MaxReturnableQty)
                    {
                        item.ReturnQuantity = item.MaxReturnableQty;
                        textBox.Text = item.MaxReturnableQty.ToString();
                    }
                    else if (qty > 0)
                    {
                        // Ensure checkbox is ticked
                        if (!item.IsSelected)
                        {
                            item.IsSelected = true;
                        }
                    }
                    else if (qty == 0)
                    {
                        // Uncheck if 0
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                    }
                }
                else
                {
                    // Invalid input, reset to 0
                    item.ReturnQuantity = 0;
                    textBox.Text = "0";
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                }
                
                UpdateSummary();
            }
        }

        private void UpdateSummary()
        {
            decimal totalRefund = 0;
            int itemCount = 0;

            foreach (var item in _returnItems)
            {
                if (item.IsSelected && item.ReturnQuantity > 0)
                {
                    if (item.ReturnQuantity > item.MaxReturnableQty)
                    {
                        item.ReturnQuantity = item.MaxReturnableQty;
                    }

                    totalRefund += item.ReturnQuantity * item.UnitPrice;
                    itemCount++;
                }
            }

            if (txtItemCount != null)
                txtItemCount.Text = itemCount.ToString();
            
            if (txtRefundAmount != null)
                txtRefundAmount.Text = $"Rs {totalRefund:#,##0.00}";
        }

        private async void ProcessReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItems = _returnItems.Where(i => i.IsSelected && i.ReturnQuantity > 0).ToList();

                if (!selectedItems.Any())
                {
                    MessageBox.Show("Please select at least one item to return.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal totalRefund = selectedItems.Sum(i => i.ReturnQuantity * i.UnitPrice);

                var result = MessageBox.Show(
                    $"CONFIRM RETURN\n\n" +
                    $"Invoice: {_invoice.InvoiceNumber}\n" +
                    $"Items to Return: {selectedItems.Count}\n" +
                    $"Refund Amount: Rs {totalRefund:#,##0.00}\n\n" +
                    $"Process this return?",
                    "Confirm Return",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // Get condition status
                var conditionText = ((ComboBoxItem)cmbCondition.SelectedItem).Content.ToString();
                var conditionStatus = conditionText.Contains("Good") ? ReturnCondition.Good : ReturnCondition.Damaged;

                // Get or create default user and session
                int userId = await EnsureUserExistsAsync();
                int sessionId = await EnsureSessionExistsAsync(userId);

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                // Create SalesReturn
                var salesReturn = new SalesReturn
                {
                    SaleId = _invoice.SaleId,
                    UserId = userId,
                    SessionId = sessionId,
                    ReturnDate = DateTime.Now,
                    TotalRefund = totalRefund,
                    Reason = ((ComboBoxItem)cmbReason.SelectedItem).Content.ToString()
                };

                await _unitOfWork.Repository<SalesReturn>().AddAsync(salesReturn);
                await _unitOfWork.SaveChangesAsync();

                // Create SalesReturnItems and update inventory
                foreach (var item in selectedItems)
                {
                    var returnItem = new SalesReturnItem
                    {
                        ReturnId = salesReturn.ReturnId,
                        SaleItemId = item.SaleItemId,
                        ProductId = item.ProductId,
                        Quantity = item.ReturnQuantity,
                        RefundAmount = item.ReturnQuantity * item.UnitPrice,
                        ConditionStatus = conditionStatus
                    };

                    await _unitOfWork.Repository<SalesReturnItem>().AddAsync(returnItem);

                    // Update SalesItem.QuantityReturned
                    var saleItem = await _unitOfWork.Repository<SalesItem>().GetByIdAsync(item.SaleItemId);
                    if (saleItem != null)
                    {
                        saleItem.QuantityReturned += item.ReturnQuantity;
                        _unitOfWork.Repository<SalesItem>().Update(saleItem);
                    }

                    // Update inventory (add back to stock if Good condition)
                    if (conditionStatus == ReturnCondition.Good)
                    {
                        var inventory = await _unitOfWork.Repository<Inventory>()
                            .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                        if (inventory != null)
                        {
                            inventory.Quantity += item.ReturnQuantity;
                            _unitOfWork.Repository<Inventory>().Update(inventory);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                DialogResult = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                MessageBox.Show($"Error processing return:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task<int> EnsureUserExistsAsync()
        {
            try
            {
                var users = await _unitOfWork.Repository<User>().GetAllAsync();
                var firstUser = users.FirstOrDefault();
                return firstUser?.UserId ?? 1;
            }
            catch
            {
                return 1;
            }
        }

        private async System.Threading.Tasks.Task<int> EnsureSessionExistsAsync(int userId)
        {
            try
            {
                var sessions = await _unitOfWork.Repository<DrawerSession>().GetAllAsync();
                var openSession = sessions.FirstOrDefault(s => s.Status == SessionStatus.Open);

                if (openSession != null)
                {
                    return openSession.SessionId;
                }

                var defaultSession = new DrawerSession
                {
                    UserId = userId,
                    OpeningCash = 0,
                    StartTime = DateTime.Now,
                    Status = SessionStatus.Open
                };

                await _unitOfWork.Repository<DrawerSession>().AddAsync(defaultSession);
                await _unitOfWork.SaveChangesAsync();

                return defaultSession.SessionId;
            }
            catch
            {
                return 1;
            }
        }
    }
}
