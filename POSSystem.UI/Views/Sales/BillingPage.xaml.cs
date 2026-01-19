using POSSystem.Application.DTOs;
using POSSystem.Application.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.Printing;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace POSSystem.UI.Views.Sales
{
    public partial class BillingPage : Window
    {
        private readonly IProductService _productService;
        private readonly ISalesService _salesService;

        // For product grid
        public ObservableCollection<ProductDisplayModel> DisplayProducts { get; set; }

        // For bill items
        public ObservableCollection<BillItem> BillItems { get; set; }

        // Properties for bill calculations
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }

        // Current user and session (hardcoded for now, will be from login later)
        private readonly int _currentUserId = 1; // Admin user
        private readonly int _currentSessionId = 1; // Default session

        public BillingPage(IProductService productService, ISalesService salesService)
        {
            InitializeComponent();

            _productService = productService;
            _salesService = salesService;

            // Initialize collections
            BillItems = new ObservableCollection<BillItem>();
            DisplayProducts = new ObservableCollection<ProductDisplayModel>();

            // Set DataContext
            DataContext = this;

            // Bind DataGrid
            dgBillItems.ItemsSource = BillItems;

            // Load products from database
            LoadProductsAsync();

            // Update UI
            UpdateBillSummary();
        }

        private async void LoadProductsAsync()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                
                DisplayProducts.Clear();
                foreach (var product in products)
                {
                    DisplayProducts.Add(new ProductDisplayModel
                    {
                        Id = product.ProductId.ToString(),
                        Name = product.Name,
                        Category = product.Category,
                        Price = product.SellingPrice,
                        Stock = (int)product.Quantity
                    });
                }

                // Set ProductsList items source
                ProductsList.ItemsSource = DisplayProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== EVENT HANDLERS ====================

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize window when loaded
            txtCurrentTime.Text = DateTime.Now.ToString("hh:mm tt");
            txtBillNo.Text = "BILL-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string productId = button.Tag.ToString();
                var product = DisplayProducts.FirstOrDefault(p => p.Id == productId);

                if (product != null)
                {
                    // Check if item already exists in bill
                    var existingItem = BillItems.FirstOrDefault(i => i.ProductId == productId);

                    if (existingItem != null)
                    {
                        // Increase quantity
                        existingItem.Quantity++;
                        existingItem.Total = existingItem.Quantity * existingItem.Price;
                    }
                    else
                    {
                        // Add new item
                        var billItem = new BillItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = product.Id,
                            SNo = BillItems.Count + 1,
                            Name = product.Name,
                            Price = product.Price,
                            Quantity = 1,
                            Total = product.Price
                        };
                        BillItems.Add(billItem);
                    }

                    // Update UI
                    UpdateBillSummary();
                }
            }
        }

        private void ProductItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Alternative way to add product - double click
            var border = sender as Border;
            if (border != null && border.DataContext is ProductDisplayModel product)
            {
                // Add product to bill
                AddProductToBill(product);
            }
        }

        private void AddProductToBill(ProductDisplayModel product)
        {
            var existingItem = BillItems.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                existingItem.Total = existingItem.Quantity * existingItem.Price;
            }
            else
            {
                var billItem = new BillItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    SNo = BillItems.Count + 1,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    Total = product.Price
                };
                BillItems.Add(billItem);
            }

            UpdateBillSummary();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string itemId = button.Tag.ToString();
                var itemToRemove = BillItems.FirstOrDefault(i => i.Id == itemId);

                if (itemToRemove != null)
                {
                    BillItems.Remove(itemToRemove);

                    // Update serial numbers
                    for (int i = 0; i < BillItems.Count; i++)
                    {
                        BillItems[i].SNo = i + 1;
                    }

                    UpdateBillSummary();
                }
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all items?",
                                                    "Clear All Items",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                BillItems.Clear();
                UpdateBillSummary();
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgBillItems.SelectedItem is BillItem selectedItem)
            {
                BillItems.Remove(selectedItem);

                // Update serial numbers
                for (int i = 0; i < BillItems.Count; i++)
                {
                    BillItems[i].SNo = i + 1;
                }

                UpdateBillSummary();
            }
            else
            {
                MessageBox.Show("Please select an item to remove", "No Item Selected",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e)
        {
            if (dgBillItems.SelectedItem is BillItem selectedItem)
            {
                // Create a simple dialog to edit quantity
                var dialog = new Window
                {
                    Title = "Edit Quantity",
                    Width = 300,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Editing: {selectedItem.Name}",
                    Margin = new Thickness(0, 0, 0, 10),
                    FontWeight = FontWeights.Bold
                });

                var quantityTextBox = new TextBox
                {
                    Text = selectedItem.Quantity.ToString(),
                    Margin = new Thickness(0, 0, 0, 20),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Margin = new Thickness(5)
                };

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Margin = new Thickness(5)
                };

                okButton.Click += (s, args) =>
                {
                    if (int.TryParse(quantityTextBox.Text, out int newQuantity) && newQuantity > 0)
                    {
                        selectedItem.Quantity = newQuantity;
                        selectedItem.Total = selectedItem.Quantity * selectedItem.Price;
                        UpdateBillSummary();
                        dialog.Close();
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid quantity", "Invalid Input",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };

                cancelButton.Click += (s, args) => dialog.Close();

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                stackPanel.Children.Add(quantityTextBox);
                stackPanel.Children.Add(buttonPanel);

                dialog.Content = stackPanel;
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an item to edit", "No Item Selected",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateBillSummary()
        {
            // Calculate totals
            SubTotal = BillItems.Sum(item => item.Total);

            // Apply discount
            if (decimal.TryParse(txtDiscountInput.Text, out decimal discountInput))
            {
                if (cmbDiscountType.SelectedIndex == 1) // Percentage
                {
                    Discount = SubTotal * discountInput / 100;
                }
                else // Fixed amount
                {
                    Discount = discountInput;
                }
            }

            // Calculate tax (18% on subtotal minus discount)
            Tax = (SubTotal - Discount) * 0.18m;

            // Calculate grand total
            GrandTotal = SubTotal - Discount + Tax;

            // Update UI
            txtSubTotal.Text = $"Rs {SubTotal:#,##0.00}";
            txtDiscount.Text = $"Rs {Discount:#,##0.00}";
            txtTax.Text = $"Rs {Tax:#,##0.00}";
            txtGrandTotal.Text = $"Rs {GrandTotal:#,##0.00}";

            // Update item count
            txtItemCount.Text = BillItems.Count.ToString();
            txtBillItemsCount.Text = $"{BillItems.Count} Items";

            // Update total amount in header
            txtTotalAmount.Text = $"Rs {GrandTotal:#,##0.00}";
        }

        private void txtDiscountInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBillSummary();
        }

        private void cmbDiscountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBillSummary();
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            // Set amount paid to grand total
            txtAmountPaid.Text = GrandTotal.ToString("0.00");
        }

        private void btnNewBill_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Start a new bill? Current bill will be cleared.",
                                                    "New Bill",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                BillItems.Clear();
                txtDiscountInput.Text = "0";
                txtAmountPaid.Text = "0.00";
                txtBillNo.Text = "BILL-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                UpdateBillSummary();
            }
        }

        private void btnHoldBill_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bill held successfully! You can retrieve it from 'Hold Bills'",
                          "Bill Held",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void btnPrintBill_Click(object sender, RoutedEventArgs e)
        {
            if (BillItems.Count == 0)
            {
                MessageBox.Show("No items in the bill to print", "Empty Bill",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get payment details
            if (!decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                MessageBox.Show("Please enter amount paid before printing", "Amount Required",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal change = amountPaid - GrandTotal;
            string paymentMethod = GetSelectedPaymentMethod();

            // Create invoice
            Invoice invoice = CreateInvoiceFromCurrentBill(amountPaid, change, paymentMethod);

            // Ask for print format
            MessageBoxResult formatResult = MessageBox.Show("Select Bill Format:\n\nYes - Relaword Format (Thermal)\nNo - Standard Format (A4)",
                                                          "Print Format",
                                                          MessageBoxButton.YesNoCancel,
                                                          MessageBoxImage.Question);

            if (formatResult == MessageBoxResult.Yes)
            {
                // Print in Relaword format
                PrintRelawordBill(invoice);
            }
            else if (formatResult == MessageBoxResult.No)
            {
                // Print in standard format
                PrintSuperBill(invoice);
            }
        }

        // ==================== UPDATED PROCESS PAYMENT WITH DATABASE ====================
        private async void btnProcessPayment_Click(object sender, RoutedEventArgs e)
        {
            if (BillItems.Count == 0)
            {
                MessageBox.Show("No items in the bill to process", "Empty Bill",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                MessageBox.Show("Please enter a valid amount", "Invalid Amount",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmountPaid.Focus();
                return;
            }

            if (amountPaid < GrandTotal)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Amount paid (₹ {amountPaid:#,##0.00}) is less than grand total (₹ {GrandTotal:#,##0.00}). Continue?",
                    "Insufficient Amount",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    txtAmountPaid.Focus();
                    return;
                }
            }

            try
            {
                // Disable button to prevent double-clicking
                btnProcessPayment.IsEnabled = false;

                decimal change = amountPaid - GrandTotal;
                string paymentMethod = GetSelectedPaymentMethod();

                // Create sale DTO
                var saleDto = new CreateSaleDto
                {
                    CustomerId = null, // Walk-in customer
                    UserId = _currentUserId,
                    SessionId = _currentSessionId,
                    SubTotal = SubTotal,
                    TaxAmount = Tax,
                    DiscountAmount = Discount,
                    GrandTotal = GrandTotal,
                    PaymentMethod = paymentMethod,
                    AmountPaid = amountPaid,
                    Items = BillItems.Select(bi => new CreateSaleItemDto
                    {
                        ProductId = int.Parse(bi.ProductId),
                        Quantity = bi.Quantity,
                        UnitPrice = bi.Price
                    }).ToList()
                };

                // Save sale to database
                var result = await _salesService.CreateSaleAsync(saleDto);

                if (!result.Success)
                {
                    MessageBox.Show($"Error saving sale: {result.Message}", "Sale Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    btnProcessPayment.IsEnabled = true;
                    return;
                }

                // Create invoice for printing
                Invoice invoice = CreateInvoiceFromCurrentBill(amountPaid, change, paymentMethod);
                invoice.InvoiceNumber = $"INV-{result.SaleId:D6}";

                // Show success message
                string message = $"✅ PAYMENT SUCCESSFUL!\n\n" +
                               $"Invoice: {invoice.InvoiceNumber}\n" +
                               $"Total: ₹ {GrandTotal:#,##0.00}\n" +
                               $"Paid: ₹ {amountPaid:#,##0.00}\n" +
                               $"Change: ₹ {change:#,##0.00}\n" +
                               $"Method: {paymentMethod}\n\n" +
                               $"✅ Sale saved to database!\n" +
                               $"✅ Inventory updated!";

                MessageBoxResult printResult = MessageBox.Show(message + "\n\nPrint Relaword format bill?",
                                                             "Payment Complete",
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Information);

                if (printResult == MessageBoxResult.Yes)
                {
                    // Auto-print in Relaword format
                    PrintRelawordBill(invoice);
                }

                // Clear bill and start new
                BillItems.Clear();
                txtDiscountInput.Text = "0";
                txtAmountPaid.Text = "0.00";
                txtBillNo.Text = "BILL-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                UpdateBillSummary();

                // Reload products to show updated stock
                LoadProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnProcessPayment.IsEnabled = true;
            }
        }

        private void btnBarcode_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Barcode scanner activated!\n\nPress F2 to scan or click OK to enter manually",
                          "Barcode Scanner",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnHoldList_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Showing held bills...", "Hold Bills",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnInvoiceHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Showing invoice history...", "Invoice History",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCustomerMgmt_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening customer management...", "Customer Management",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Previous page", "Navigation",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Next page", "Navigation",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================== PRINTING METHODS ====================
        private string GetSelectedPaymentMethod()
        {
            if (rbCash.IsChecked == true) return "Cash";
            if (rbCard.IsChecked == true) return "Card";
            if (rbUPI.IsChecked == true) return "UPI";
            if (rbCredit.IsChecked == true) return "Credit";
            return "Cash";
        }

        private Invoice CreateInvoiceFromCurrentBill(decimal amountPaid, decimal change, string paymentMethod)
        {
            var invoice = new Invoice
            {
                InvoiceNumber = txtBillNo.Text,
                InvoiceDate = DateTime.Now,
                CustomerName = txtCustomerName.Text,
                CustomerPhone = "Not Provided",
                CustomerAddress = "Walk-in Customer",
                CashierName = "Admin",
                PaymentMethod = paymentMethod,
                SubTotal = SubTotal,
                Discount = Discount,
                Tax = Tax,
                GrandTotal = GrandTotal,
                AmountPaid = amountPaid,
                Change = change,
                Items = new ObservableCollection<InvoiceItem>()
            };

            // Add bill items to invoice
            foreach (BillItem item in BillItems)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    SNo = item.SNo,
                    ProductName = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Total = item.Total
                });
            }

            return invoice;
        }

        private void PrintSuperBill(Invoice invoice)
        {
            try
            {
                FlowDocument document = CreateSuperBillDocument(invoice);

                PrintDialog printDialog = new PrintDialog();

                // Set printer preferences
                printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
                printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;

                // Show print dialog
                if (printDialog.ShowDialog() == true)
                {
                    IDocumentPaginatorSource idpSource = document;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, $"POS Invoice - {invoice.InvoiceNumber}");

                    MessageBox.Show("Bill printed successfully!", "Print Complete",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}\nPlease check printer connection.",
                              "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateSuperBillDocument(Invoice invoice)
        {
            FlowDocument flowDoc = new FlowDocument();
            flowDoc.PagePadding = new Thickness(20);
            flowDoc.ColumnWidth = double.MaxValue;

            Paragraph para = new Paragraph();

            // =============== HEADER SECTION ===============
            para.Inlines.Add(new Run("🛒 POS PRO SUPER STORE")
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue
            });
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run("123 Retail Street, Business City") { FontSize = 11 });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("📞 +91 98765 43210 | ✉️ info@pospro.com") { FontSize = 10 });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("🕒 Open: 9AM-10PM | GSTIN: 27AAAAA0000A1Z5") { FontSize = 9 });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // =============== INVOICE HEADER ===============
            para.Inlines.Add(new Run("══════════ TAX INVOICE ══════════")
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Purple
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // Invoice Details
            para.Inlines.Add(new Run($"Invoice No: {invoice.InvoiceNumber}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Date: {invoice.InvoiceDate:dd-MM-yyyy HH:mm:ss}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Customer: {invoice.CustomerName}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Phone: {invoice.CustomerPhone}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // =============== ITEMS SECTION ===============
            para.Inlines.Add(new Run("══════════ ITEMS PURCHASED ══════════")
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // Simple items list
            foreach (var item in invoice.Items)
            {
                para.Inlines.Add(new Run($"{item.SNo}. {item.ProductName}"));
                para.Inlines.Add(new LineBreak());
                para.Inlines.Add(new Run($"   Qty: {item.Quantity} × Rs {item.UnitPrice:#,##0.00} = Rs {item.Total:#,##0.00}"));
                para.Inlines.Add(new LineBreak());
            }

            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // =============== TOTALS SECTION ===============
            para.Inlines.Add(new Run("══════════ BILL SUMMARY ══════════")
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run($"Sub Total:    Rs {invoice.SubTotal:#,##0.00}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Discount:     Rs {invoice.Discount:#,##0.00}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Tax (18%):    Rs {invoice.Tax:#,##0.00}"));
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run($"GRAND TOTAL:  Rs {invoice.GrandTotal:#,##0.00}")
            {
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                FontSize = 14
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run($"Amount Paid:  Rs {invoice.AmountPaid:#,##0.00}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Change:       Rs {invoice.Change:#,##0.00}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // =============== PAYMENT INFO ===============
            para.Inlines.Add(new Run("══════════ PAYMENT INFO ══════════")
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold
            });
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run($"Payment Method: {invoice.PaymentMethod}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Transaction ID: TXN-{DateTime.Now:yyyyMMddHHmmss}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Status: ✅ PAID"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Cashier: {invoice.CashierName}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run($"Print Time: {DateTime.Now:HH:mm:ss}"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            // =============== FOOTER SECTION ===============
            para.Inlines.Add(new Run("══════════ THANK YOU! ══════════")
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run("📢 IMPORTANT NOTES:") { FontWeight = FontWeights.Bold });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("• Goods sold are not returnable or exchangeable"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("• Warranty as per manufacturer's terms"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("• Please check items at the time of purchase"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("• Keep this bill for warranty claims"));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run("🏪 Visit Again! We Value Your Business")
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new LineBreak());

            para.Inlines.Add(new Run("═══════════════════════════════════")
            {
                FontSize = 10,
                Foreground = Brushes.DarkGray
            });
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Run("⚡ Computer Generated Invoice - Valid Without Signature")
            {
                FontSize = 8,
                FontStyle = FontStyles.Italic
            });

            flowDoc.Blocks.Add(para);
            return flowDoc;
        }

        // ==================== RELAWORD BILL PRINTING ====================
        private void PrintRelawordBill(Invoice invoice)
        {
            try
            {
                // Create Relaword style bill document
                FlowDocument document = CreateRelawordBillDocument(invoice);

                PrintDialog printDialog = new PrintDialog();

                // Set paper size for receipt (80mm thermal paper)
                printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA7);
                printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;

                if (printDialog.ShowDialog() == true)
                {
                    IDocumentPaginatorSource idpSource = document;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, $"Bill - {invoice.InvoiceNumber}");

                    MessageBox.Show("Bill printed in Relaword format!", "Print Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing error: {ex.Message}", "Print Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateRelawordBillDocument(Invoice invoice)
        {
            FlowDocument flowDoc = new FlowDocument();
            flowDoc.PagePadding = new Thickness(15, 10, 15, 10);
            flowDoc.ColumnWidth = 280; // 80mm width in pixels
            flowDoc.FontFamily = new FontFamily("Consolas, Courier New");
            flowDoc.FontSize = 10;

            // Create a container for the table
            Paragraph container = new Paragraph();

            // =============== RELAWORD HEADER ===============
            container.Inlines.Add(new Run(new string('*', 40)) { FontWeight = FontWeights.Bold });
            container.Inlines.Add(new LineBreak());

            // Center align store name
            container.Inlines.Add(new Run("        🛍️ RELAWORD STORES        ")
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue
            });
            container.Inlines.Add(new LineBreak());

            container.Inlines.Add(new Run("----------------------------------------"));
            container.Inlines.Add(new LineBreak());

            // Store details in Indian format
            container.Inlines.Add(new Run("📌 123, Gandhi Road, Chennai - 600001"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("📞 044-12345678 | 📱 +91 9876543210"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("✉️ relaword@email.com"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("GSTIN: 33AAAAA0000A1Z5"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("----------------------------------------"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // =============== BILL HEADER ===============
            container.Inlines.Add(new Run("          ॐ TAX INVOICE ॐ          ")
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Purple
            });
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // Bill details in table format
            container.Inlines.Add(new Run("Bill No   : " + invoice.InvoiceNumber));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Date      : " + invoice.InvoiceDate.ToString("dd/MM/yyyy")));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Time      : " + invoice.InvoiceDate.ToString("hh:mm:ss tt")));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Customer  : " + invoice.CustomerName));
            container.Inlines.Add(new LineBreak());

            if (!string.IsNullOrEmpty(invoice.CustomerPhone) && invoice.CustomerPhone != "Not Provided")
            {
                container.Inlines.Add(new Run("Mobile    : " + invoice.CustomerPhone));
                container.Inlines.Add(new LineBreak());
            }

            container.Inlines.Add(new Run("----------------------------------------"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // =============== ITEMS SECTION ===============
            container.Inlines.Add(new Run("Sr  Item Description          Qty    Rate     Amount"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("--------------------------------------------------------"));
            container.Inlines.Add(new LineBreak());

            foreach (var item in invoice.Items)
            {
                string itemName = item.ProductName;
                if (itemName.Length > 20)
                    itemName = itemName.Substring(0, 17) + "...";

                string line = $"{item.SNo,2}  {itemName,-20} {item.Quantity,4} {item.UnitPrice,7:0.00} {item.Total,9:0.00}";
                container.Inlines.Add(new Run(line));
                container.Inlines.Add(new LineBreak());
            }

            container.Inlines.Add(new Run("--------------------------------------------------------"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // =============== TOTALS SECTION ===============
            container.Inlines.Add(new Run("Sub Total".PadRight(30) + ":" + invoice.SubTotal.ToString("0.00").PadLeft(10)));
            container.Inlines.Add(new LineBreak());

            if (invoice.Discount > 0)
            {
                container.Inlines.Add(new Run("Discount".PadRight(30) + ":-" + invoice.Discount.ToString("0.00").PadLeft(9)));
                container.Inlines.Add(new LineBreak());
            }

            container.Inlines.Add(new Run("GST @18%".PadRight(30) + ":" + invoice.Tax.ToString("0.00").PadLeft(10)));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("----------------------------------------"));
            container.Inlines.Add(new LineBreak());

            // Grand Total (Highlighted)
            container.Inlines.Add(new Run("GRAND TOTAL".PadRight(30) + ":"));
            container.Inlines.Add(new Run("₹ " + invoice.GrandTotal.ToString("0.00").PadLeft(8))
            {
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                FontSize = 11
            });
            container.Inlines.Add(new LineBreak());

            container.Inlines.Add(new Run("========================================"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // Payment details
            container.Inlines.Add(new Run("PAYMENT DETAILS"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Mode".PadRight(20) + ": " + invoice.PaymentMethod));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Paid".PadRight(20) + ": ₹ " + invoice.AmountPaid.ToString("0.00")));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Change".PadRight(20) + ": ₹ " + invoice.Change.ToString("0.00")));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("----------------------------------------"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // =============== FOOTER SECTION ===============
            // Cashier info
            container.Inlines.Add(new Run("Cashier".PadRight(15) + ": " + invoice.CashierName));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("Counter".PadRight(15) + ": 01"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // Thank you message
            container.Inlines.Add(new Run("          धन्यवाद ! THANK YOU !          ")
            {
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green
            });
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // Terms and conditions
            container.Inlines.Add(new Run("✪ Terms & Conditions:"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("• Goods once sold will not be taken back"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("• Warranty as per manufacturer policy"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("• Please check items at time of purchase"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("• Bill must be produced for exchange"));
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new LineBreak());

            // Final separator
            container.Inlines.Add(new Run(new string('=', 40)) { FontWeight = FontWeights.Bold });
            container.Inlines.Add(new LineBreak());

            // Computer generated note
            container.Inlines.Add(new Run("      🖥️ COMPUTER GENERATED BILL      ")
            {
                FontSize = 8,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });
            container.Inlines.Add(new LineBreak());
            container.Inlines.Add(new Run("         (Valid without signature)         ")
            {
                FontSize = 8,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });
            container.Inlines.Add(new LineBreak());

            // Cut here line for thermal printers
            container.Inlines.Add(new Run("- - - - - - - CUT HERE - - - - - - -")
            {
                FontSize = 9,
                Foreground = Brushes.DarkGray
            });

            flowDoc.Blocks.Add(container);
            return flowDoc;
        }

        // ==================== SUPPORTING CLASSES ====================
        public class ProductDisplayModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }

        public class BillItem
        {
            public string Id { get; set; }
            public string ProductId { get; set; }
            public int SNo { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Total { get; set; }
        }

        // ==================== INVOICE CLASSES ====================
        public class Invoice
        {
            public string InvoiceNumber { get; set; }
            public DateTime InvoiceDate { get; set; }
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public string CustomerAddress { get; set; }
            public ObservableCollection<InvoiceItem> Items { get; set; }
            public decimal SubTotal { get; set; }
            public decimal Discount { get; set; }
            public decimal Tax { get; set; }
            public decimal GrandTotal { get; set; }
            public decimal AmountPaid { get; set; }
            public decimal Change { get; set; }
            public string PaymentMethod { get; set; }
            public string CashierName { get; set; }
        }

        public class InvoiceItem
        {
            public int SNo { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
        }
    }
}