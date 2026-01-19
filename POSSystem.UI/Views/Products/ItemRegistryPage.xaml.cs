using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace POSSystem.UI.Views.Products
{
    public partial class ItemRegistryPage : Window
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        public ObservableCollection<ProductItemViewModel> AllProducts { get; set; }
        public ObservableCollection<ProductItemViewModel> FilteredProducts { get; set; }

        public ItemRegistryPage()
        {
            // Initialize collections FIRST before InitializeComponent
            AllProducts = new ObservableCollection<ProductItemViewModel>();
            FilteredProducts = new ObservableCollection<ProductItemViewModel>();

            InitializeComponent();

            // Get services from DI container
            var app = (App)System.Windows.Application.Current;
            _productService = app.ServiceProvider.GetRequiredService<IProductService>();
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Set DataContext
            DataContext = this;

            // Load data when window is fully loaded
            this.Loaded += ItemRegistryPage_Loaded;
        }

        private void ItemRegistryPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProductsAsync();
        }

        private async void LoadProductsAsync()
        {
            try
            {
                // Show loading
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Visible;

                // Fetch all products
                var products = await _productService.GetAllProductsAsync();

                AllProducts.Clear();
                FilteredProducts.Clear();

                foreach (var product in products)
                {
                    var item = new ProductItemViewModel
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Barcode = product.Barcode ?? "N/A",
                        Category = product.Category,
                        SellingPrice = product.SellingPrice,
                        Quantity = (int)product.Quantity,
                        AlertQty = product.AlertQty,
                        Status = product.IsActive ? "Active" : "Inactive"
                    };

                    AllProducts.Add(item);
                    FilteredProducts.Add(item);
                }

                // Bind to DataGrid
                if (ProductsDataGrid != null)
                {
                    ProductsDataGrid.ItemsSource = FilteredProducts;
                }

                // Update statistics
                UpdateStatistics();

                // Update status
                if (StatusText != null)
                    StatusText.Text = "Ready";

                if (LastUpdatedText != null)
                    LastUpdatedText.Text = $"Last Updated: {DateTime.Now:hh:mm tt}";

                // Hide loading
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateStatistics()
        {
            int totalProducts = AllProducts.Count;
            int activeProducts = AllProducts.Count(p => p.Status == "Active");

            if (TotalProductsText != null)
                TotalProductsText.Text = totalProducts.ToString();

            if (ActiveProductsText != null)
                ActiveProductsText.Text = activeProducts.ToString();

            if (RecordCountText != null)
                RecordCountText.Text = $"{FilteredProducts.Count} of {totalProducts} products displayed";
        }

        private void AddNewProductButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProductDialogAdvanced(_productService, _unitOfWork);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                LoadProductsAsync();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox == null || searchBox.Text == "Search by Product Name, Product ID, or Barcode...")
            {
                FilteredProducts.Clear();
                foreach (var product in AllProducts)
                {
                    FilteredProducts.Add(product);
                }
                UpdateStatistics();
                return;
            }

            string searchTerm = searchBox.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                FilteredProducts.Clear();
                foreach (var product in AllProducts)
                {
                    FilteredProducts.Add(product);
                }
                UpdateStatistics();
                return;
            }

            // Filter based on search type
            int searchType = SearchTypeComboBox?.SelectedIndex ?? 0;

            FilteredProducts.Clear();

            foreach (var product in AllProducts)
            {
                bool matches = searchType switch
                {
                    0 => // All Fields
                        product.ProductName.ToLower().Contains(searchTerm) ||
                        product.ProductId.ToString().Contains(searchTerm) ||
                        (product.Barcode?.ToLower().Contains(searchTerm) ?? false),

                    1 => // Product Name only
                        product.ProductName.ToLower().Contains(searchTerm),

                    2 => // Product ID only
                        product.ProductId.ToString().Contains(searchTerm),

                    3 => // Barcode only
                        (product.Barcode?.ToLower().Contains(searchTerm) ?? false),

                    _ => false
                };

                if (matches)
                {
                    FilteredProducts.Add(product);
                }
            }

            UpdateStatistics();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Search by Product Name, Product ID, or Barcode...")
            {
                textBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Search by Product Name, Product ID, or Barcode...";
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox != null)
            {
                SearchBox.Text = "Search by Product Name, Product ID, or Barcode...";
            }

            if (SearchTypeComboBox != null)
            {
                SearchTypeComboBox.SelectedIndex = 0;
            }

            FilteredProducts.Clear();
            foreach (var product in AllProducts)
            {
                FilteredProducts.Add(product);
            }

            UpdateStatistics();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProductsAsync();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel/PDF functionality will be implemented here.",
                "Export Products", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print product list functionality will be implemented here.",
                "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // ViewModel for Product Items
    public class ProductItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public int AlertQty { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Advanced Add Product Dialog with Category/SubCategory Dropdowns
    public class AddProductDialogAdvanced : Window
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;

        private TextBox txtProductName;
        private TextBox txtBarcode;
        private TextBox txtDescription;
        private ComboBox cmbCategory;
        private ComboBox cmbSubCategory;
        private TextBox txtSellingPrice;
        private TextBox txtCostPrice;
        private TextBox txtInitialStock;
        private TextBox txtAlertQty;
        private TextBox txtUnitMeasure;
        private CheckBox chkIsActive;

        private List<Category> _categories = new List<Category>();
        private List<SubCategory> _allSubCategories = new List<SubCategory>();

        public AddProductDialogAdvanced(IProductService productService, IUnitOfWork unitOfWork)
        {
            _productService = productService;
            _unitOfWork = unitOfWork;

            Title = "Add New Product - Advanced";
            Width = 600;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 46));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(30)
            };

            var mainPanel = new StackPanel();

            // Title
            var title = new TextBlock
            {
                Text = "? Add New Product",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(title);

            // Product Name
            AddLabel(mainPanel, "Product Name: *");
            txtProductName = AddTextBox(mainPanel);

            // Barcode/SKU
            AddLabel(mainPanel, "Barcode/SKU:");
            txtBarcode = AddTextBox(mainPanel);

            // Description
            AddLabel(mainPanel, "Description:");
            txtDescription = AddTextBox(mainPanel, "", 60);

            // Category Dropdown
            AddLabel(mainPanel, "Category: *");
            cmbCategory = AddComboBox(mainPanel);
            cmbCategory.SelectionChanged += CmbCategory_SelectionChanged;

            // SubCategory Dropdown
            AddLabel(mainPanel, "SubCategory: *");
            cmbSubCategory = AddComboBox(mainPanel);

            // Selling Price
            AddLabel(mainPanel, "Selling Price (Rs): *");
            txtSellingPrice = AddTextBox(mainPanel);

            // Cost Price
            AddLabel(mainPanel, "Cost Price (Rs):");
            txtCostPrice = AddTextBox(mainPanel, "0");

            // Initial Stock
            AddLabel(mainPanel, "Initial Stock Quantity: *");
            txtInitialStock = AddTextBox(mainPanel, "0");

            // Alert Quantity
            AddLabel(mainPanel, "Alert Quantity (Low Stock): *");
            txtAlertQty = AddTextBox(mainPanel, "10");

            // Unit Measure
            AddLabel(mainPanel, "Unit of Measure:");
            txtUnitMeasure = AddTextBox(mainPanel, "pcs");

            // Is Active Checkbox
            chkIsActive = new CheckBox
            {
                Content = "Active Product",
                IsChecked = true,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 13,
                Margin = new Thickness(0, 15, 0, 20)
            };
            mainPanel.Children.Add(chkIsActive);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 20)
            };

            var saveButton = new Button
            {
                Content = "?? Save Product",
                Width = 140,
                Height = 45,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 93, 255)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 10, 0)
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 45,
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 14
            };
            cancelButton.Click += (s, args) => DialogResult = false;

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            scrollViewer.Content = mainPanel;
            Content = scrollViewer;

            // Load categories
            LoadCategoriesAsync();
        }

        private async void LoadCategoriesAsync()
        {
            try
            {
                _categories = (await _unitOfWork.Repository<Category>().GetAllAsync()).ToList();
                _allSubCategories = (await _unitOfWork.Repository<SubCategory>().GetAllAsync()).ToList();

                cmbCategory.ItemsSource = _categories;
                cmbCategory.DisplayMemberPath = "Name";
                cmbCategory.SelectedValuePath = "CategoryId";

                if (_categories.Any())
                {
                    cmbCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedItem is Category selectedCategory)
            {
                var subCategories = _allSubCategories.Where(sc => sc.CategoryId == selectedCategory.CategoryId).ToList();

                cmbSubCategory.ItemsSource = subCategories;
                cmbSubCategory.DisplayMemberPath = "Name";
                cmbSubCategory.SelectedValuePath = "SubCatId";

                if (subCategories.Any())
                {
                    cmbSubCategory.SelectedIndex = 0;
                }
            }
        }

        private void AddLabel(StackPanel panel, string text)
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 176, 208)),
                Margin = new Thickness(0, 12, 0, 5)
            });
        }

        private TextBox AddTextBox(StackPanel panel, string defaultValue = "", int height = 40)
        {
            var textBox = new TextBox
            {
                Height = height,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 64)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                FontSize = 14,
                Text = defaultValue,
                TextWrapping = height > 40 ? TextWrapping.Wrap : TextWrapping.NoWrap,
                AcceptsReturn = height > 40
            };
            panel.Children.Add(textBox);
            return textBox;
        }

        private ComboBox AddComboBox(StackPanel panel)
        {
            var comboBox = new ComboBox
            {
                Height = 40,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 64)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 58, 94)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 14
            };

            // Set style for dropdown items to have dark background and white text
            var itemContainerStyle = new Style(typeof(ComboBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, 
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 64))));
            itemContainerStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, 
                System.Windows.Media.Brushes.White));
            
            // Hover effect
            var hoverTrigger = new Trigger { Property = ComboBoxItem.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(ComboBoxItem.BackgroundProperty, 
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 58, 94))));
            itemContainerStyle.Triggers.Add(hoverTrigger);
            
            comboBox.ItemContainerStyle = itemContainerStyle;

            panel.Children.Add(comboBox);
            return comboBox;
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

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a category", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSubCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a subcategory", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out decimal sellingPrice) || sellingPrice <= 0)
            {
                MessageBox.Show("Please enter a valid selling price", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCostPrice.Text, out decimal costPrice) || costPrice < 0)
            {
                costPrice = 0;
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
                var selectedSubCat = cmbSubCategory.SelectedItem as SubCategory;

                var productDto = new CreateProductDto
                {
                    Name = txtProductName.Text.Trim(),
                    Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                    SubCatId = selectedSubCat?.SubCatId,
                    SellingPrice = sellingPrice,
                    InitialQuantity = initialStock,
                    AlertQty = alertQty,
                    UnitMeasure = string.IsNullOrWhiteSpace(txtUnitMeasure.Text) ? "pcs" : txtUnitMeasure.Text.Trim()
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
}
