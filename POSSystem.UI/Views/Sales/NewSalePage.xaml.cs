using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSystem.UI.Views.Sales
{
    public partial class NewSalePage : Window
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private readonly IUnitOfWork _unitOfWork;

        private ObservableCollection<SaleItemViewModel> _saleItems;
        private ObservableCollection<ProductSuggestionViewModel> _productSuggestions;
        private List<ProductDto> _allProducts = new List<ProductDto>();
        private decimal _subTotal = 0;
        private decimal _discountAmount = 0;
        private decimal _taxAmount = 0;
        private decimal _grandTotal = 0;
        private int _itemCounter = 1;

        public NewSalePage()
        {
            // Initialize collections first
            _saleItems = new ObservableCollection<SaleItemViewModel>();
            _productSuggestions = new ObservableCollection<ProductSuggestionViewModel>();
            
            InitializeComponent();

            // Get services from DI
            var app = (App)System.Windows.Application.Current;
            _productService = app.ServiceProvider.GetRequiredService<IProductService>();
            _salesService = app.ServiceProvider.GetRequiredService<ISalesService>();
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Set DataGrid source
            dgSaleItems.ItemsSource = _saleItems;
            ProductSuggestionsList.ItemsSource = _productSuggestions;

            // Set defaults (after InitializeComponent)
            txtInvoiceNumber.Text = GenerateInvoiceNumber();
            dpSaleDate.SelectedDate = DateTime.Now;
            txtTime.Text = DateTime.Now.ToString("hh:mm tt");

            // Initialize calculation fields
            UpdateCalculations();

            // Load all products for suggestions
            LoadProductsAsync();

            // Start timer for live time update
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => txtTime.Text = DateTime.Now.ToString("hh:mm tt");
            timer.Start();
        }

        private async void LoadProductsAsync()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                _allProducts = products.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}";
        }

        private async void ProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if popup is initialized
            if (ProductSuggestionsPopup == null || _productSuggestions == null)
                return;

            var searchText = txtProductSearch.Text.Trim();

            // Hide popup if empty
            if (string.IsNullOrWhiteSpace(searchText))
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
                    SellingPrice = p.SellingPrice,
                    AvailableQty = (int)p.Quantity,
                    Product = p
                })
                .ToList();
        }

        private void ProductSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear placeholder if needed
        }

        private void ProductSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            // Close popup when focus is lost (with delay to allow click)
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += (s, args) =>
            {
                if (ProductSuggestionsPopup != null)
                    ProductSuggestionsPopup.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        private void SuggestionItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProductSuggestionViewModel suggestion)
            {
                SelectProductFromSuggestion(suggestion.Product);
                if (ProductSuggestionsPopup != null)
                    ProductSuggestionsPopup.IsOpen = false;
            }
        }

        private void SelectProductFromSuggestion(ProductDto product)
        {
            txtProductSearch.Text = product.Name;
            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }

        private void ProductSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // If suggestions are open, select first one
                if (ProductSuggestionsPopup != null && ProductSuggestionsPopup.IsOpen && _productSuggestions.Count > 0)
                {
                    SelectProductFromSuggestion(_productSuggestions[0].Product);
                    ProductSuggestionsPopup.IsOpen = false;
                }
                
                AddItemButton_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                if (ProductSuggestionsPopup != null)
                    ProductSuggestionsPopup.IsOpen = false;
            }
        }

        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchTerm = txtProductSearch.Text.Trim();
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    MessageBox.Show("Please enter product ID, barcode, or name.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
                {
                    MessageBox.Show("Please enter a valid quantity.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Search product by ID first, then barcode, then name
                ProductDto? product = null;

                if (int.TryParse(searchTerm, out int productId))
                {
                    product = await _productService.GetProductByIdAsync(productId);
                }

                if (product == null)
                {
                    product = await _productService.GetProductByBarcodeAsync(searchTerm);
                }

                if (product == null)
                {
                    var products = await _productService.SearchProductsAsync(searchTerm);
                    product = products.FirstOrDefault();
                }

                if (product == null)
                {
                    MessageBox.Show($"Product '{searchTerm}' not found.", "Not Found",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check stock availability
                if (product.Quantity < quantity)
                {
                    MessageBox.Show($"Insufficient stock. Available: {product.Quantity}", "Stock Alert",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if item already exists
                var existingItem = _saleItems.FirstOrDefault(i => i.ProductId == product.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                }
                else
                {
                    // Add new item
                    var saleItem = new SaleItemViewModel
                    {
                        SNo = _itemCounter++,
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        UnitPrice = product.SellingPrice, // Using selling price for sales
                        Quantity = quantity,
                        TotalPrice = quantity * product.SellingPrice
                    };

                    _saleItems.Add(saleItem);
                }

                // Refresh grid and calculations
                dgSaleItems.Items.Refresh();
                UpdateCalculations();

                // Clear search
                txtProductSearch.Clear();
                txtQuantity.Text = "1";
                txtProductSearch.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                int productId = (int)button.Tag;
                var item = _saleItems.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    _saleItems.Remove(item);
                    UpdateCalculations();
                    dgSaleItems.Items.Refresh();
                }
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Clear all sale items?", "Confirm Clear",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _saleItems.Clear();
                _itemCounter = 1;
                UpdateCalculations();
                dgSaleItems.Items.Refresh();
            }
        }

        private void Discount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void Tax_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void AmountPaid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtAmountPaid == null || txtChange == null) return; // Guard against null controls
            
            if (decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                decimal change = amountPaid - _grandTotal;
                txtChange.Text = $"Rs {change:#,##0.00}";
                
                try
                {
                    txtChange.Foreground = change >= 0 ? 
                        (System.Windows.Media.Brush)FindResource("SuccessGreen") : 
                        (System.Windows.Media.Brush)FindResource("ErrorRed");
                }
                catch
                {
                    // Fallback if resources not loaded
                    txtChange.Foreground = System.Windows.Media.Brushes.White;
                }
            }
        }

        private void UpdateCalculations()
        {
            // Guard against null controls during initialization
            if (_saleItems == null || txtDiscountPercent == null || txtTaxPercent == null ||
                txtItemCount == null || txtSubTotal == null || txtDiscountAmount == null ||
                txtTaxAmount == null || txtGrandTotal == null)
            {
                return;
            }

            // Calculate subtotal
            _subTotal = _saleItems.Sum(i => i.TotalPrice);

            // Calculate discount
            decimal.TryParse(txtDiscountPercent.Text, out decimal discountPercent);
            _discountAmount = _subTotal * (discountPercent / 100);

            // Calculate tax on (subtotal - discount)
            decimal.TryParse(txtTaxPercent.Text, out decimal taxPercent);
            _taxAmount = (_subTotal - _discountAmount) * (taxPercent / 100);

            // Calculate grand total
            _grandTotal = _subTotal - _discountAmount + _taxAmount;

            // Update UI
            txtItemCount.Text = _saleItems.Count.ToString();
            txtSubTotal.Text = $"Rs {_subTotal:#,##0.00}";
            txtDiscountAmount.Text = $"Rs {_discountAmount:#,##0.00}";
            txtTaxAmount.Text = $"Rs {_taxAmount:#,##0.00}";
            txtGrandTotal.Text = $"Rs {_grandTotal:#,##0.00}";

            // Update change
            if (txtAmountPaid != null)
            {
                AmountPaid_TextChanged(null, null);
            }
        }

        private async void CompleteSaleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (_saleItems.Count == 0)
                {
                    MessageBox.Show("Please add items to the sale.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
                {
                    MessageBox.Show("Please enter amount paid.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (amountPaid < _grandTotal)
                {
                    var confirmResult = MessageBox.Show($"Insufficient payment.\nRequired: Rs {_grandTotal:#,##0.00}\nPaid: Rs {amountPaid:#,##0.00}\n\nCreate credit sale?",
                        "Payment Alert", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirmResult == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                // Get payment method
                string paymentMethod = "Cash";
                if (rbCard.IsChecked == true) paymentMethod = "Card";
                else if (rbTransfer.IsChecked == true) paymentMethod = "Transfer";
                else if (rbCredit.IsChecked == true) paymentMethod = "Credit";

                // Get or create default user and session
                int userId = await EnsureUserExistsAsync();
                int sessionId = await EnsureSessionExistsAsync(userId);

                // Create sale DTO
                var createSaleDto = new CreateSaleDto
                {
                    CustomerId = null, // Walk-in customer
                    UserId = userId,
                    SessionId = sessionId,
                    SubTotal = _subTotal,
                    TaxAmount = _taxAmount,
                    DiscountAmount = _discountAmount,
                    GrandTotal = _grandTotal,
                    PaymentMethod = paymentMethod,
                    AmountPaid = amountPaid,
                    Items = _saleItems.Select(item => new CreateSaleItemDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                // Process sale
                var saleResult = await _salesService.CreateSaleAsync(createSaleDto);

                if (saleResult.Success)
                {
                    string message = $"? SALE COMPLETED SUCCESSFULLY!\n\n" +
                                   $"Invoice: INV-{saleResult.SaleId:D6}\n" +
                                   $"Total: Rs {_grandTotal:#,##0.00}\n" +
                                   $"Paid: Rs {amountPaid:#,##0.00}\n" +
                                   $"Change: Rs {saleResult.Change:#,##0.00}\n" +
                                   $"Payment: {paymentMethod}\n\n" +
                                   $"? Sale saved to database!\n" +
                                   $"? Inventory updated!\n" +
                                   $"? Payment recorded!";

                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset for new sale
                    ResetSale();
                }
                else
                {
                    MessageBox.Show($"Error: {saleResult.Message}", "Sale Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing sale:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<int> EnsureUserExistsAsync()
        {
            try
            {
                // Try to get existing users
                var users = await _unitOfWork.Repository<User>().GetAllAsync();
                var firstUser = users.FirstOrDefault();

                if (firstUser != null)
                {
                    return firstUser.UserId;
                }

                // Create default admin user if none exists
                var defaultUser = new User
                {
                    Username = "admin",
                    PasswordHash = "admin", // In production, this should be properly hashed!
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Repository<User>().AddAsync(defaultUser);
                await _unitOfWork.SaveChangesAsync();

                return defaultUser.UserId;
            }
            catch
            {
                // If User table doesn't exist or there's an error, return default ID
                return 1;
            }
        }

        private async Task<int> EnsureSessionExistsAsync(int userId)
        {
            try
            {
                // Try to get existing open sessions
                var sessions = await _unitOfWork.Repository<DrawerSession>().GetAllAsync();
                var openSession = sessions.FirstOrDefault(s => s.Status == SessionStatus.Open);

                if (openSession != null)
                {
                    return openSession.SessionId;
                }

                // Create default session if none exists
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
                // If DrawerSession table doesn't exist or there's an error, return default ID
                return 1;
            }
        }

        private void ResetSale()
        {
            _saleItems.Clear();
            _itemCounter = 1;
            txtInvoiceNumber.Text = GenerateInvoiceNumber();
            txtProductSearch.Clear();
            txtQuantity.Text = "1";
            txtCustomerName.Text = "Walk-in Customer";
            txtCustomerPhone.Clear();
            txtDiscountPercent.Text = "0";
            txtTaxPercent.Text = "0";
            txtAmountPaid.Text = "0.00";
            rbCash.IsChecked = true;
            UpdateCalculations();
            dgSaleItems.Items.Refresh();
        }

        private void SaveHoldButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save & Hold functionality - Coming Soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PrintPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print Preview - Coming Soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_saleItems.Count > 0)
            {
                var result = MessageBox.Show("You have unsaved items. Are you sure you want to go back?",
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

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

    // ViewModel for Sale Items
    public class SaleItemViewModel
    {
        public int SNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ViewModel for Product Suggestions
    public class ProductSuggestionViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public int AvailableQty { get; set; }
        public ProductDto Product { get; set; } = null!;
    }
}
