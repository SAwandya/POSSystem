using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using POSSystem.UI.Views.Products;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSystem.UI.Views.GRN
{
    public partial class GRNPage : Window
    {
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        private ObservableCollection<GRNItemViewModel> _grnItems;
        private int _itemCounter = 1;
        private List<POSSystem.Domain.Entities.Supplier> _suppliers = new List<POSSystem.Domain.Entities.Supplier>();
        private Product? _selectedProduct = null;
        private List<Product> _allProducts = new List<Product>();
        private ObservableCollection<ProductSuggestionViewModel> _productSuggestions;

        public GRNPage()
        {
            InitializeComponent();

            // Get services from DI container
            var app = (App)System.Windows.Application.Current;
            _productService = app.ServiceProvider.GetRequiredService<IProductService>();
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Initialize collections
            _grnItems = new ObservableCollection<GRNItemViewModel>();
            _productSuggestions = new ObservableCollection<ProductSuggestionViewModel>();
            
            dgGRNItems.ItemsSource = _grnItems;
            ProductSuggestionsList.ItemsSource = _productSuggestions;

            // Set defaults
            dpGRNDate.SelectedDate = DateTime.Now;
            txtGRNNumber.Text = GenerateGRNNumber();

            // Load data
            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            try
            {
                // Load All Products
                var products = await _unitOfWork.Repository<Product>().GetAllAsync();
                _allProducts = products.ToList();

                // Load Suppliers
                _suppliers = (await _unitOfWork.Repository<POSSystem.Domain.Entities.Supplier>().GetAllAsync()).ToList();
                cmbSupplier.ItemsSource = _suppliers;
                cmbSupplier.DisplayMemberPath = "Name";
                cmbSupplier.SelectedValuePath = "SupplierId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateGRNNumber()
        {
            return $"GRN-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private void TxtProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if popup is initialized
            if (ProductSuggestionsPopup == null || _productSuggestions == null)
                return;

            var searchText = txtProductSearch.Text.Trim();

            // Hide popup if placeholder text
            if (searchText == "Search by barcode, product ID, or name..." || string.IsNullOrWhiteSpace(searchText))
            {
                ProductSuggestionsPopup.IsOpen = false;
                return;
            }

            // Search products
            var suggestions = SearchProducts(searchText);

            _productSuggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                _productSuggestions.Add(suggestion);
            }

            // Show/hide popup based on results
            ProductSuggestionsPopup.IsOpen = _productSuggestions.Count > 0;
        }

        private List<ProductSuggestionViewModel> SearchProducts(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            return _allProducts
                .Where(p =>
                    p.ProductId.ToString().Contains(searchTerm) ||
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)))
                .Take(10) // Limit to 10 suggestions
                .Select(p => new ProductSuggestionViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Barcode = p.Barcode ?? "N/A",
                    CategoryName = p.SubCategory?.Category?.Name ?? "N/A",
                    AvailableQty = (int)(p.Inventory?.Quantity ?? 0),
                    Product = p
                })
                .ToList();
        }

        private void TxtProductSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtProductSearch.Text == "Search by barcode, product ID, or name...")
            {
                txtProductSearch.Text = "";
                txtProductSearch.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        private void TxtProductSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductSearch.Text))
            {
                txtProductSearch.Text = "Search by barcode, product ID, or name...";
                txtProductSearch.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 176, 208));
            }
        }

        private void TxtProductSearch_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if popup is initialized
            if (ProductSuggestionsPopup == null)
                return;

            if (e.Key == Key.Enter)
            {
                // Select first suggestion if available
                if (_productSuggestions.Count > 0)
                {
                    SelectProduct(_productSuggestions[0].Product);
                    ProductSuggestionsPopup.IsOpen = false;
                }
            }
            else if (e.Key == Key.Escape)
            {
                ProductSuggestionsPopup.IsOpen = false;
            }
        }

        private void SuggestionItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProductSuggestionViewModel suggestion)
            {
                SelectProduct(suggestion.Product);
                ProductSuggestionsPopup.IsOpen = false;
            }
        }

        private void SelectProduct(Product product)
        {
            _selectedProduct = product;
            txtProductSearch.Text = $"{product.Name} (ID: {product.ProductId})";
            txtAvailableQty.Text = (product.Inventory?.Quantity ?? 0).ToString();
            
            // Auto-fill unit price from Product.UnitPrice
            if (product.UnitPrice > 0)
            {
                txtUnitPrice.Text = product.UnitPrice.ToString("0.00");
            }
            else
            {
                txtUnitPrice.Text = "0.00";
            }
            
            // Focus on quantity field
            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }

        private void AddNewProductButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProductDialogAdvanced(_productService, _unitOfWork);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                // Reload products
                LoadInitialDataAsync();
                MessageBox.Show("Product added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchItemsButton_Click(object sender, RoutedEventArgs e)
        {
            var itemRegistry = new ItemRegistryPage();
            itemRegistry.Owner = this;
            itemRegistry.Show();
        }

        private void SelectSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSupplier.SelectedItem is POSSystem.Domain.Entities.Supplier supplier)
            {
                txtSupplierId.Text = supplier.SupplierId.ToString();
                MessageBox.Show($"Supplier selected: {supplier.Name}", "Supplier",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a supplier from dropdown first.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addSupplierDialog = new POSSystem.UI.Views.Suppliers.AddSupplierDialog(_unitOfWork);
                addSupplierDialog.Owner = this;

                if (addSupplierDialog.ShowDialog() == true)
                {
                    // Reload suppliers after adding
                    _suppliers = (await _unitOfWork.Repository<POSSystem.Domain.Entities.Supplier>().GetAllAsync()).ToList();
                    cmbSupplier.ItemsSource = _suppliers;
                    cmbSupplier.DisplayMemberPath = "Name";
                    cmbSupplier.SelectedValuePath = "SupplierId";

                    // Select the newly added supplier (last one in the list)
                    if (_suppliers.Any())
                    {
                        cmbSupplier.SelectedIndex = _suppliers.Count - 1;
                    }

                    MessageBox.Show("Supplier added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding supplier: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (_selectedProduct == null)
            {
                MessageBox.Show("Please search and select a product first.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                discount = 0;
            }

            // Get unit price from the textbox
            decimal unitPrice = 0;
            if (!decimal.TryParse(txtUnitPrice.Text, out unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Please enter a valid unit price.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUnitPrice.Focus();
                return;
            }

            // Calculate total
            decimal subtotal = quantity * unitPrice;
            decimal discountAmount = subtotal * (discount / 100);
            decimal total = subtotal - discountAmount;

            // Create GRN item
            var grnItem = new GRNItemViewModel
            {
                SNo = _itemCounter++,
                ProductId = _selectedProduct.ProductId,
                ProductName = _selectedProduct.Name,
                Category = _selectedProduct.SubCategory?.Category?.Name ?? "N/A",
                SubCategory = _selectedProduct.SubCategory?.Name ?? "N/A",
                Quantity = quantity,
                UnitPrice = unitPrice,
                DiscountPercent = discount,
                Total = total
            };

            _grnItems.Add(grnItem);
            UpdateSummary();

            // Reset for next item
            _selectedProduct = null;
            txtProductSearch.Text = "Search by barcode, product ID, or name...";
            txtProductSearch.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 176, 208));
            txtQuantity.Text = "0";
            txtUnitPrice.Text = "0.00";
            txtDiscount.Text = "0";
            txtAvailableQty.Text = "0";
            chkSingle.IsChecked = false;
            chkPacked.IsChecked = false;

            txtProductSearch.Focus();
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgGRNItems.SelectedItem is GRNItemViewModel selectedItem)
            {
                _grnItems.Remove(selectedItem);
                UpdateSummary();
            }
            else
            {
                MessageBox.Show("Please select an item to remove.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateSummary()
        {
            txtItemCount.Text = _grnItems.Count.ToString();
            decimal subtotal = _grnItems.Sum(i => i.Total);
            txtSubtotal.Text = $"Rs {subtotal:#,##0.00}";
        }

        private void PayGRNButton_Click(object sender, RoutedEventArgs e)
        {
            if (_grnItems.Count == 0)
            {
                MessageBox.Show("Please add items before completing GRN.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Please select a supplier.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Complete this GRN and update stock?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveGRNAsync();
            }
        }

        private async void SaveGRNAsync()
        {
            try
            {
                var supplier = cmbSupplier.SelectedItem as POSSystem.Domain.Entities.Supplier;
                if (supplier == null)
                {
                    MessageBox.Show("Please select a supplier.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create GRN record
                var grn = new POSSystem.Domain.Entities.GRN
                {
                    SupplierId = supplier.SupplierId,
                    UserId = 1, // Default user ID - should be from logged-in user
                    ReferenceNo = txtReferenceNo.Text,
                    ReceivedDate = dpGRNDate.SelectedDate ?? DateTime.Now,
                    TotalAmount = _grnItems.Sum(i => i.Total)
                };

                await _unitOfWork.Repository<POSSystem.Domain.Entities.GRN>().AddAsync(grn);
                await _unitOfWork.SaveChangesAsync();

                // Create GRN items and update inventory
                foreach (var item in _grnItems)
                {
                    // Create GRN item
                    var grnItem = new GRNItem
                    {
                        GrnId = grn.GrnId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitCost = item.UnitPrice,
                        TotalCost = item.Total
                    };

                    await _unitOfWork.Repository<GRNItem>().AddAsync(grnItem);

                    // Update inventory quantity
                    var inventory = await _unitOfWork.Repository<Inventory>()
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                    if (inventory != null)
                    {
                        // Add received quantity to existing inventory
                        inventory.Quantity += item.Quantity;
                        
                        // Update average cost
                        var oldTotalCost = inventory.AverageCost * (inventory.Quantity - item.Quantity);
                        var newTotalCost = oldTotalCost + item.Total;
                        inventory.AverageCost = inventory.Quantity > 0 ? newTotalCost / inventory.Quantity : item.UnitPrice;
                        
                        _unitOfWork.Repository<Inventory>().Update(inventory);
                    }
                    else
                    {
                        // Create new inventory record if it doesn't exist
                        var newInventory = new Inventory
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            SellingPrice = 0, // To be set separately
                            AverageCost = item.UnitPrice
                        };
                        await _unitOfWork.Repository<Inventory>().AddAsync(newInventory);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                MessageBox.Show($"GRN {txtGRNNumber.Text} completed successfully!\n\n" +
                               $"Total Items: {_grnItems.Count}\n" +
                               $"Total Amount: Rs {grn.TotalAmount:#,##0.00}\n" +
                               $"Supplier: {supplier.Name}\n\n" +
                               $"Inventory has been updated!", 
                               "Success",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload products to show updated inventory
                LoadInitialDataAsync();
                
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving GRN: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", 
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all data?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ResetForm();
            }
        }

        private void ResetForm()
        {
            _grnItems.Clear();
            _itemCounter = 1;
            txtGRNNumber.Text = GenerateGRNNumber();
            txtPONumber.Clear();
            txtInvoiceNumber.Clear();
            txtInvoiceValue.Clear();
            txtReferenceNo.Clear();
            txtProductSearch.Text = "Search by barcode, product ID, or name...";
            txtProductSearch.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(176, 176, 208));
            txtQuantity.Text = "0";
            txtUnitPrice.Text = "0.00";
            txtDiscount.Text = "0";
            txtOutstanding.Clear();
            txtAvailableQty.Text = "0";
            cmbSupplier.SelectedIndex = -1;
            dpGRNDate.SelectedDate = DateTime.Now;
            _selectedProduct = null;
            
            // Check if popup is initialized before accessing
            if (ProductSuggestionsPopup != null)
                ProductSuggestionsPopup.IsOpen = false;
                
            UpdateSummary();
        }

        private void ViewTempButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View temporary GRN data.", "View Temp",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DelTempButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete temporary GRN data.", "Delete Temp",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_grnItems.Count > 0)
            {
                var result = MessageBox.Show("You have unsaved changes. Are you sure you want to go back?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
    }

    // ViewModel for GRN Items
    public class GRNItemViewModel
    {
        public int SNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Total { get; set; }
    }

    // ViewModel for Product Suggestions
    public class ProductSuggestionViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int AvailableQty { get; set; }
        public Product Product { get; set; } = null!;
    }
}