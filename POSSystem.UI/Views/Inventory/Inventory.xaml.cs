using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace POSSystem.UI.Views.inventory
{
    public partial class Inventory : Window
    {
        private readonly IProductService _productService;
        public ObservableCollection<InventoryItemViewModel> InventoryItems { get; set; }

        public Inventory()
        {
            InitializeComponent();

            // Get service from DI container
            var app = (App)System.Windows.Application.Current;
            _productService = app.ServiceProvider.GetRequiredService<IProductService>();

            // Initialize collection
            InventoryItems = new ObservableCollection<InventoryItemViewModel>();

            // Set DataContext
            DataContext = this;

            // Load data when window is fully loaded
            this.Loaded += Inventory_Loaded;
        }

        private void Inventory_Loaded(object sender, RoutedEventArgs e)
        {
            // Load data from database after UI is fully initialized
            LoadInventoryAsync();
        }

        private async void LoadInventoryAsync()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();

                InventoryItems.Clear();
                int itemId = 1;

                foreach (var product in products)
                {
                    var item = new InventoryItemViewModel
                    {
                        ItemID = itemId++,
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Category = product.Category,
                        SKU = product.Barcode ?? $"SKU-{product.ProductId:D6}",
                        CurrentStock = (int)product.Quantity,
                        MinStock = product.AlertQty,
                        Price = product.SellingPrice
                    };

                    // Set status based on stock levels
                    if (item.CurrentStock == 0)
                    {
                        item.Status = "Out of Stock";
                        item.StatusColor = new SolidColorBrush(Color.FromRgb(255, 94, 124)); // ErrorRed
                    }
                    else if (item.CurrentStock <= item.MinStock)
                    {
                        item.Status = "Low Stock";
                        item.StatusColor = new SolidColorBrush(Color.FromRgb(255, 209, 102)); // WarningYellow
                    }
                    else
                    {
                        item.Status = "In Stock";
                        item.StatusColor = new SolidColorBrush(Color.FromRgb(0, 224, 150)); // SuccessGreen
                    }

                    InventoryItems.Add(item);
                }

                // Bind to DataGrid (now it's fully initialized)
                if (InventoryDataGrid != null)
                {
                    InventoryDataGrid.ItemsSource = InventoryItems;
                }

                // Update stats
                UpdateStatistics();

                // Update last updated time
                if (LastUpdatedText != null)
                {
                    LastUpdatedText.Text = $"Last Updated: {DateTime.Now:hh:mm tt}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            int totalItems = InventoryItems.Count;
            int lowStock = InventoryItems.Count(i => i.Status == "Low Stock");
            int outOfStock = InventoryItems.Count(i => i.Status == "Out of Stock");
            decimal totalValue = InventoryItems.Sum(i => i.CurrentStock * i.Price);

            // Update UI stats with null checks
            if (TotalItemsValue != null)
                TotalItemsValue.Text = totalItems.ToString();
            
            if (LowStockValue != null)
                LowStockValue.Text = lowStock.ToString();
            
            if (OutOfStockValue != null)
                OutOfStockValue.Text = outOfStock.ToString();
            
            if (TotalValueText != null)
                TotalValueText.Text = $"Total Value: Rs {totalValue:#,##0.00}";
        }

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProductDialog(_productService);
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                // Reload inventory after adding
                LoadInventoryAsync();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInventoryAsync();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export functionality will export inventory to Excel/PDF",
                "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Filter options:\n- By Category\n- By Stock Status\n- By Price Range",
                "Filter", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StockInButton_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryDataGrid.SelectedItem is InventoryItemViewModel selectedItem)
            {
                var dialog = new StockAdjustmentDialog(selectedItem, _productService, true);
                dialog.Owner = this;
                
                if (dialog.ShowDialog() == true)
                {
                    LoadInventoryAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the inventory list",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StockOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryDataGrid.SelectedItem is InventoryItemViewModel selectedItem)
            {
                var dialog = new StockAdjustmentDialog(selectedItem, _productService, false);
                dialog.Owner = this;
                
                if (dialog.ShowDialog() == true)
                {
                    LoadInventoryAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the inventory list",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox == null || string.IsNullOrWhiteSpace(searchBox.Text) || searchBox.Text == "Search items...")
            {
                if (InventoryDataGrid != null)
                {
                    InventoryDataGrid.ItemsSource = InventoryItems;
                }
                return;
            }

            string searchTerm = searchBox.Text.ToLower();
            var filtered = InventoryItems.Where(i =>
                i.ProductName.ToLower().Contains(searchTerm) ||
                i.SKU.ToLower().Contains(searchTerm) ||
                i.Category.ToLower().Contains(searchTerm)
            ).ToList();

            if (InventoryDataGrid != null)
            {
                InventoryDataGrid.ItemsSource = filtered;
            }
        }
    }

    // ViewModel for Inventory Items
    public class InventoryItemViewModel
    {
        public int ItemID { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public Brush StatusColor { get; set; } = Brushes.Green;
    }

    // Dialog for adding new products
    public class AddProductDialog : Window
    {
        private readonly IProductService _productService;
        private TextBox txtProductName;
        private TextBox txtBarcode;
        private TextBox txtSellingPrice;
        private TextBox txtInitialStock;
        private TextBox txtAlertQty;

        public AddProductDialog(IProductService productService)
        {
            _productService = productService;

            Title = "Add New Product";
            Width = 500;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(26, 26, 46));

            var mainPanel = new StackPanel { Margin = new Thickness(30) };

            // Title
            var title = new TextBlock
            {
                Text = "➕ Add New Product",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Product Name
            AddLabel(mainPanel, "Product Name:");
            txtProductName = AddTextBox(mainPanel);

            // Barcode
            AddLabel(mainPanel, "Barcode/SKU:");
            txtBarcode = AddTextBox(mainPanel);

            // Selling Price
            AddLabel(mainPanel, "Selling Price (Rs):");
            txtSellingPrice = AddTextBox(mainPanel);

            // Initial Stock
            AddLabel(mainPanel, "Initial Stock Quantity:");
            txtInitialStock = AddTextBox(mainPanel);

            // Alert Quantity
            AddLabel(mainPanel, "Alert Quantity (Low Stock):");
            txtAlertQty = AddTextBox(mainPanel, "10");

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var saveButton = new Button
            {
                Content = "💾 Save Product",
                Width = 130,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(139, 93, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 40,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancelButton.Click += (s, args) => DialogResult = false;

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            Content = mainPanel;
        }

        private void AddLabel(StackPanel panel, string text)
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 208)),
                Margin = new Thickness(0, 10, 0, 5)
            });
        }

        private TextBox AddTextBox(StackPanel panel, string defaultValue = "")
        {
            var textBox = new TextBox
            {
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 64)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                FontSize = 14,
                Text = defaultValue
            };
            panel.Children.Add(textBox);
            return textBox;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter product name", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out decimal sellingPrice) || sellingPrice <= 0)
            {
                MessageBox.Show("Please enter a valid selling price", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtInitialStock.Text, out decimal initialStock) || initialStock < 0)
            {
                MessageBox.Show("Please enter a valid stock quantity", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtAlertQty.Text, out int alertQty) || alertQty < 0)
            {
                MessageBox.Show("Please enter a valid alert quantity", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var productDto = new CreateProductDto
                {
                    Name = txtProductName.Text.Trim(),
                    Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim(),
                    SellingPrice = sellingPrice,
                    InitialQuantity = initialStock,
                    AlertQty = alertQty,
                    SubCatId = 1 // Default category for now
                };

                await _productService.CreateProductAsync(productDto);
                
                MessageBox.Show("Product added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                string errorMessage = "Database error while saving product.\n\n";
                
                if (dbEx.InnerException != null)
                {
                    errorMessage += $"Details: {dbEx.InnerException.Message}\n\n";
                }
                
                errorMessage += "Possible causes:\n";
                errorMessage += "• Missing category in database (run database setup script)\n";
                errorMessage += "• Duplicate barcode/SKU\n";
                errorMessage += "• Database connection issue";
                
                MessageBox.Show(errorMessage, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product:\n\n{ex.Message}\n\nInner Exception:\n{ex.InnerException?.Message}", 
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Dialog for stock adjustments
    public class StockAdjustmentDialog : Window
    {
        private readonly InventoryItemViewModel _item;
        private readonly IProductService _productService;
        private readonly bool _isStockIn;
        private TextBox txtQuantity;
        private TextBox txtReason;

        public StockAdjustmentDialog(InventoryItemViewModel item, IProductService productService, bool isStockIn)
        {
            _item = item;
            _productService = productService;
            _isStockIn = isStockIn;

            Title = isStockIn ? "Stock In" : "Stock Out";
            Width = 450;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(26, 26, 46));

            var mainPanel = new StackPanel { Margin = new Thickness(30) };

            // Title
            var title = new TextBlock
            {
                Text = isStockIn ? "📥 Add Stock" : "📤 Remove Stock",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(title);

            // Product Info
            var productInfo = new TextBlock
            {
                Text = $"Product: {item.ProductName}\nCurrent Stock: {item.CurrentStock}",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 208)),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(productInfo);

            // Quantity
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Quantity:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 208)),
                Margin = new Thickness(0, 10, 0, 5)
            });

            txtQuantity = new TextBox
            {
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 64)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                FontSize = 14,
                Text = "1"
            };
            mainPanel.Children.Add(txtQuantity);

            // Reason
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Reason:",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 208)),
                Margin = new Thickness(0, 10, 0, 5)
            });

            txtReason = new TextBox
            {
                Height = 80,
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 64)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };
            mainPanel.Children.Add(txtReason);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var saveButton = new Button
            {
                Content = "💾 Save",
                Width = 100,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(139, 93, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 40,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancelButton.Click += (s, args) => DialogResult = false;

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            Content = mainPanel;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                decimal newStock = _isStockIn ?
                    _item.CurrentStock + quantity :
                    _item.CurrentStock - quantity;

                if (newStock < 0)
                {
                    MessageBox.Show("Cannot remove more stock than available", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _productService.UpdateStockAsync(_item.ProductId, newStock, txtReason.Text);
                
                MessageBox.Show($"Stock {(_isStockIn ? "added" : "removed")} successfully!\nNew Stock: {newStock}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating stock: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
