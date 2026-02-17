using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POSSystem.UI.Views.Dashboard;

namespace POSSystem.UI.Views.Sales
{
    public partial class NewSaleClone : Window
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;
        private readonly IUnitOfWork _unitOfWork;

        private ObservableCollection<SaleItemViewModel> _saleItems;
        private ObservableCollection<ProductSuggestionViewModel> _productSuggestions;

        private List<ProductDtoClone> _allProducts = new List<ProductDtoClone>();

        private decimal _subTotal = 0;
        private decimal _discountAmount = 0; // invoice level
        private decimal _taxAmount = 0;
        private decimal _grandTotal = 0;

        private int _itemCounter = 1;

        public NewSaleClone()
        {
            _saleItems = new ObservableCollection<SaleItemViewModel>();
            _productSuggestions = new ObservableCollection<ProductSuggestionViewModel>();

            InitializeComponent();

            var app = (App)System.Windows.Application.Current;
            _productService = app.ServiceProvider.GetRequiredService<IProductService>();
            _salesService = app.ServiceProvider.GetRequiredService<ISalesService>();
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Bind UI
            dgSaleItems.ItemsSource = _saleItems;
            ProductSuggestionsList.ItemsSource = _productSuggestions;

            // Defaults
            txtInvoiceNumber.Text = GenerateInvoiceNumber();
            dpSaleDate.SelectedDate = DateTime.Now;
            txtTime.Text = DateTime.Now.ToString("hh:mm tt");

            // Default per-item discount to 0
            if (Discount != null)
                Discount.Text = "0";

            LoadProductsAsync();

            // Calculations
            UpdateCalculations();

            // Live time update
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => txtTime.Text = DateTime.Now.ToString("hh:mm tt");
            timer.Start();
        }

        // ---------------- Load Products ----------------

        private async void LoadProductsAsync()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                _allProducts = products
                    .Select(p => new ProductDtoClone
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Barcode = p.Barcode,
                        SellingPrice = p.SellingPrice,
                        UnitPrice = p.UnitPrice,
                        Quantity = p.Quantity
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-PREVIEW-{DateTime.Now:yyyyMMddHHmmss}";
        }

        // ---------------- Search Suggestions ----------------

        private void ProductSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductSuggestionsPopup == null || _productSuggestions == null) return;

            var searchText = txtProductSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                ProductSuggestionsPopup.IsOpen = false;
                return;
            }

            var suggestions = SearchProducts(searchText);

            _productSuggestions.Clear();
            foreach (var suggestion in suggestions)
                _productSuggestions.Add(suggestion);

            ProductSuggestionsPopup.IsOpen = _productSuggestions.Count > 0;
        }

        private List<ProductSuggestionViewModel> SearchProducts(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            return _allProducts
                .Where(p =>
                    p.ProductId.ToString().Contains(searchTerm) ||
                    p.Name.ToLower().Contains(searchTerm) ||
                    (!string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode.ToLower().Contains(searchTerm)))
                .Take(10)
                .Select(p => new ProductSuggestionViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Barcode = p.Barcode ?? "N/A",
                    SellingPrice = p.SellingPrice,
                    UnitPrice = p.UnitPrice,
                    AvailableQty = (int)p.Quantity,
                    Product = p
                })
                .ToList();
        }

        private void SuggestionItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProductSuggestionViewModel suggestion)
            {
                SelectProductFromSuggestion(suggestion.Product);
                ProductSuggestionsPopup.IsOpen = false;
            }
        }

        private void SelectProductFromSuggestion(ProductDtoClone product)
        {
            txtProductSearch.Text = product.Name;
            txtItemCode.Text = product.ProductId.ToString();

            rate.Text = product.SellingPrice.ToString("0.00");
            Cost.Text = product.UnitPrice.ToString("0.00");
            txtlabelprice.Text = product.SellingPrice.ToString("0.00");

            ProductSuggestionsPopup.IsOpen = false;

            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }

        private void ProductSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
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

        private void ProductSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            // optional
        }

        private void ProductSearch_LostFocus(object sender, RoutedEventArgs e)
        {
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

        // ---------------- Helpers ----------------

        private decimal GetItemDiscountPercent()
        {
            // LEFT discount textbox is per-item discount %
            if (Discount == null) return 0m;

            decimal.TryParse(Discount.Text, out decimal percent);

            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            return percent;
        }

        private void RecalculateSaleItem(SaleItemViewModel item)
        {
            item.GrossTotal = item.Quantity * item.UnitPrice;
            item.DiscountAmount = item.GrossTotal * (item.DiscountPercent / 100m);
            item.TotalPrice = item.GrossTotal - item.DiscountAmount;
        }

        // ---------------- Sale Items ----------------

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

                // Find product
                ProductDtoClone? product = null;

                if (int.TryParse(searchTerm, out int productId))
                {
                    var dto = await _productService.GetProductByIdAsync(productId);
                    if (dto != null)
                    {
                        product = new ProductDtoClone
                        {
                            ProductId = dto.ProductId,
                            Name = dto.Name,
                            Barcode = dto.Barcode,
                            SellingPrice = dto.SellingPrice,
                            UnitPrice = dto.UnitPrice,
                            Quantity = dto.Quantity
                        };
                    }
                }

                if (product == null)
                {
                    var dto = await _productService.GetProductByBarcodeAsync(searchTerm);
                    if (dto != null)
                    {
                        product = new ProductDtoClone
                        {
                            ProductId = dto.ProductId,
                            Name = dto.Name,
                            Barcode = dto.Barcode,
                            SellingPrice = dto.SellingPrice,
                            UnitPrice = dto.UnitPrice,
                            Quantity = dto.Quantity
                        };
                    }
                }

                if (product == null)
                {
                    var products = await _productService.SearchProductsAsync(searchTerm);
                    var dto = products.FirstOrDefault();
                    if (dto != null)
                    {
                        product = new ProductDtoClone
                        {
                            ProductId = dto.ProductId,
                            Name = dto.Name,
                            Barcode = dto.Barcode,
                            SellingPrice = dto.SellingPrice,
                            UnitPrice = dto.UnitPrice,
                            Quantity = dto.Quantity
                        };
                    }
                }

                if (product == null)
                {
                    MessageBox.Show($"Product '{searchTerm}' not found.", "Not Found",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Stock check
                if (product.Quantity < quantity)
                {
                    MessageBox.Show($"Insufficient stock. Available: {product.Quantity}", "Stock Alert",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Read per-item discount % from left-side textbox
                decimal itemDiscountPercent = GetItemDiscountPercent();

                // Add or update existing
                var existingItem = _saleItems.FirstOrDefault(i => i.ProductId == product.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.DiscountPercent = itemDiscountPercent;
                    RecalculateSaleItem(existingItem);
                }
                else
                {
                    var saleItem = new SaleItemViewModel
                    {
                        SNo = _itemCounter++,
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        UnitPrice = product.SellingPrice,
                        CostPrice = product.UnitPrice,
                        Quantity = quantity,
                        DiscountPercent = itemDiscountPercent
                    };

                    RecalculateSaleItem(saleItem);
                    _saleItems.Add(saleItem);
                }

                dgSaleItems.Items.Refresh();
                UpdateCalculations();

                txtProductSearch.Clear();
                txtQuantity.Text = "1";
                Discount.Text = "0";
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
                int productId = Convert.ToInt32(button.Tag);

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

        // ---------------- Calculations ----------------

        private void Discount_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This is RIGHT SIDE invoice discount (txtDiscountPercent)
            UpdateCalculations();
        }

        private void Tax_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void AmountPaid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtAmountPaid == null || txtChange == null) return;

            if (decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                decimal change = amountPaid - _grandTotal;
                txtChange.Text = $"Rs {change:#,##0.00}";

                try
                {
                    txtChange.Foreground = change >= 0
                        ? (System.Windows.Media.Brush)FindResource("SuccessGreen")
                        : (System.Windows.Media.Brush)FindResource("ErrorRed");
                }
                catch
                {
                    txtChange.Foreground = System.Windows.Media.Brushes.White;
                }
            }
        }

        private void UpdateCalculations()
        {
            if (_saleItems == null || txtDiscountPercent == null || txtTaxPercent == null ||
                txtItemCount == null || txtSubTotal == null || txtDiscountAmount == null ||
                txtTaxAmount == null || txtGrandTotal == null)
            {
                return;
            }

            // ✅ Subtotal is sum of NET totals (after item discount)
            _subTotal = _saleItems.Sum(i => i.TotalPrice);

            decimal.TryParse(txtDiscountPercent.Text, out decimal discountPercent);
            _discountAmount = _subTotal * (discountPercent / 100m);

            decimal.TryParse(txtTaxPercent.Text, out decimal taxPercent);
            _taxAmount = (_subTotal - _discountAmount) * (taxPercent / 100m);

            _grandTotal = _subTotal - _discountAmount + _taxAmount;

            txtItemCount.Text = _saleItems.Count.ToString();
            txtSubTotal.Text = $"Rs {_subTotal:#,##0.00}";
            txtDiscountAmount.Text = $"Rs {_discountAmount:#,##0.00}";
            txtTaxAmount.Text = $"Rs {_taxAmount:#,##0.00}";
            txtGrandTotal.Text = $"Rs {_grandTotal:#,##0.00}";

            AmountPaid_TextChanged(null!, null!);
        }

        // ---------------- Complete Sale ----------------

        private async void CompleteSaleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_saleItems.Count == 0)
            {
                MessageBox.Show("Please add items to the sale.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                MessageBox.Show("Please enter a valid amount paid.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Payment Type
            string paymentType = rbOnDay.IsChecked == true ? "On Day" : "Credit";

            // Payment Method
            string paymentMethod = "Cash";
            if (rbCard.IsChecked == true) paymentMethod = "Card";
            else if (rbTransfer.IsChecked == true) paymentMethod = "Transfer";

            // Handle insufficient payment
            if (paymentType == "On Day" && amountPaid < _grandTotal)
            {
                var confirmResult = MessageBox.Show(
                    $"Insufficient payment.\nRequired: Rs {_grandTotal:#,##0.00}\nPaid: Rs {amountPaid:#,##0.00}\n\nCreate credit sale instead?",
                    "Payment Alert", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.No)
                    return;

                paymentType = "Credit";
            }

            // Get or create default user and session
            int userId = await EnsureUserExistsAsync();
            int sessionId = await EnsureSessionExistsAsync(userId);

            // Create sale DTO
            var createSaleDto = new CreateSaleDto
            {
                CustomerId = null,
                UserId = userId,
                SessionId = sessionId,

                SubTotal = _subTotal,
                TaxAmount = _taxAmount,
                DiscountAmount = _discountAmount, // invoice-level discount
                GrandTotal = _grandTotal,

                PaymentType = paymentType,
                PaymentMethod = paymentMethod,
                AmountPaid = amountPaid,

                Items = _saleItems.Select(item => new CreateSaleItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice

                    // NOTE:
                    // If your backend supports item discount fields,
                    // you should also send DiscountPercent / DiscountAmount here.
                }).ToList()
            };

            // Process sale
            var saleResult = await _salesService.CreateSaleAsync(createSaleDto);

            if (saleResult.Success)
            {
                decimal change = amountPaid - _grandTotal;

                string msg =
                    $"SALE COMPLETED (UI Preview Mode)\n\n" +
                    $"Invoice: {txtInvoiceNumber.Text}\n" +
                    $"Payment Type: {paymentType}\n" +
                    $"Payment Method: {paymentMethod}\n" +
                    $"Total: Rs {_grandTotal:#,##0.00}\n" +
                    $"Paid: Rs {amountPaid:#,##0.00}\n" +
                    $"Change: Rs {change:#,##0.00}\n\n" +
                    $"(No database save in preview mode)";

                MessageBox.Show(msg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                ResetSale();
            }
        }

        private async Task<int> EnsureUserExistsAsync()
        {
            try
            {
                var users = await _unitOfWork.Repository<User>().GetAllAsync();
                var firstUser = users.FirstOrDefault();

                if (firstUser != null)
                {
                    return firstUser.UserId;
                }

                var defaultUser = new User
                {
                    Username = "admin",
                    PasswordHash = "admin",
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
                return 1;
            }
        }

        private async Task<int> EnsureSessionExistsAsync(int userId)
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

        private void ResetSale()
        {
            _saleItems.Clear();
            _itemCounter = 1;

            txtInvoiceNumber.Text = GenerateInvoiceNumber();
            txtProductSearch.Clear();
            txtQuantity.Text = "1";

            txtDiscountPercent.Text = "0";
            txtTaxPercent.Text = "0";
            txtAmountPaid.Text = "0.00";

            // Reset per-item discount too
            Discount.Text = "0";

            rbCash.IsChecked = true;

            UpdateCalculations();
            dgSaleItems.Items.Refresh();
        }

        // ---------------- Header Buttons ----------------

        private void SaveHoldButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save & Hold - Coming Soon!", "Info",
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
                    this.Close();
            }
            else
            {
                this.Close();
            }
        }

        // Dashboard Button (Top)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dashboard = new POSSystem.UI.Views.Dashboard.Dashboard();
                dashboard.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dashboard not found.\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------- Models ----------------

        public class ProductDtoClone
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = "";
            public string? Barcode { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal UnitPrice { get; set; } // cost price
            public decimal Quantity { get; set; }
        }

        public class SaleItemViewModel
        {
            public int SNo { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";

            public decimal UnitPrice { get; set; }  // selling
            public decimal CostPrice { get; set; }  // cost

            public decimal Quantity { get; set; }

            // ✅ Item discount fields
            public decimal DiscountPercent { get; set; }
            public decimal DiscountAmount { get; set; }

            // ✅ Totals
            public decimal GrossTotal { get; set; }
            public decimal TotalPrice { get; set; } // after discount
        }

        public class ProductSuggestionViewModel
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = "";
            public string Barcode { get; set; } = "";
            public string CategoryName { get; set; } = string.Empty;

            public decimal SellingPrice { get; set; }
            public decimal UnitPrice { get; set; }
            public int AvailableQty { get; set; }

            public ProductDtoClone Product { get; set; } = null!;
        }

        private void dgSaleItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
