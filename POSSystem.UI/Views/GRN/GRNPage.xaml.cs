using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace POSSystem.UI.Views.GRN
{
    public partial class AddItemWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ItemViewModel> _items;
        private ObservableCollection<ItemViewModel> _searchResults;
        private ObservableCollection<SupplierViewModel> _suppliers;
        private bool _isSearchMode = false;
        private int _selectedIndex = -1;

        public ObservableCollection<ItemViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ObservableCollection<SupplierViewModel> Suppliers
        {
            get => _suppliers;
            set
            {
                _suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        public AddItemWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadInitialData();
            SetupDataGrid();
        }

        private void LoadInitialData()
        {
            // Load mock data
            Items = new ObservableCollection<ItemViewModel>();
            _searchResults = new ObservableCollection<ItemViewModel>();

            // Load suppliers
            Suppliers = new ObservableCollection<SupplierViewModel>
            {
                new SupplierViewModel { Id = "SUP001", Name = "ABC Hardware Suppliers" },
                new SupplierViewModel { Id = "SUP002", Name = "XYZ Tools & Equipment" },
                new SupplierViewModel { Id = "SUP003", Name = "Building Materials Co" }
            };
            cmbSupplier.ItemsSource = Suppliers;
            cmbSupplier.DisplayMemberPath = "Name";
            cmbSupplier.SelectedValuePath = "Id";

            // Set default values
            dpDate.SelectedDate = DateTime.Now;
            UpdateSummary();
        }

        private void SetupDataGrid()
        {
            var collectionView = CollectionViewSource.GetDefaultView(Items);
            dgItems.ItemsSource = collectionView;
        }

        private void TxtItemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtItemName.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                _isSearchMode = false;
                dgItems.ItemsSource = Items;
                txtSearchInfo.Visibility = Visibility.Collapsed;
                return;
            }

            _isSearchMode = true;

            // Search in mock database
            var results = SearchItems(searchText);
            _searchResults.Clear();
            foreach (var item in results)
            {
                _searchResults.Add(new ItemViewModel
                {
                    Id = $"search-{item.ItemCode}",
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Subcategory = item.Subcategory,
                    Unit = item.Unit,
                    Quantity = 0,
                    CostPrice = item.CostPrice,
                    SellingPrice = item.SellingPrice,
                    LabelPrice = item.LabelPrice,
                    AvailableQuantity = item.AvailableQuantity,
                    IsSearchResult = true,
                    OriginalItem = item
                });
            }

            dgItems.ItemsSource = _searchResults;

            if (_searchResults.Any())
            {
                txtSearchInfo.Text = $"{_searchResults.Count} item(s) found matching \"{searchText}\". Double-click or select to load.";
                txtSearchInfo.Visibility = Visibility.Visible;
                _selectedIndex = 0;
                if (_searchResults.Count > 0)
                {
                    dgItems.SelectedIndex = 0;
                }
            }
            else
            {
                txtSearchInfo.Text = "No items found. Try different keywords.";
                txtSearchInfo.Visibility = Visibility.Visible;
            }
        }

        private ObservableCollection<MockItem> SearchItems(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();

            var mockDatabase = new ObservableCollection<MockItem>
            {
                new MockItem { ItemCode = "10001", ItemName = "Steel Hammer 500g", Subcategory = "Hand Tools", Unit = "PCS", CostPrice = 8.50m, SellingPrice = 13.99m, LabelPrice = 15.99m, AvailableQuantity = 45 },
                new MockItem { ItemCode = "10002", ItemName = "Power Drill 750W", Subcategory = "Power Tools", Unit = "PCS", CostPrice = 55.00m, SellingPrice = 79.99m, LabelPrice = 89.99m, AvailableQuantity = 12 },
                new MockItem { ItemCode = "10003", ItemName = "Wall Paint White 5L", Subcategory = "Interior Paint", Unit = "LTR", CostPrice = 28.00m, SellingPrice = 40.99m, LabelPrice = 45.99m, AvailableQuantity = 85 },
                new MockItem { ItemCode = "10004", ItemName = "Wood Screws Pack (100pcs)", Subcategory = "Fasteners", Unit = "BOX", CostPrice = 5.50m, SellingPrice = 8.99m, LabelPrice = 9.99m, AvailableQuantity = 200 },
                new MockItem { ItemCode = "10005", ItemName = "Cement Bag 50kg", Subcategory = "Building Materials", Unit = "BAG", CostPrice = 12.00m, SellingPrice = 18.99m, LabelPrice = 20.99m, AvailableQuantity = 500 },
                new MockItem { ItemCode = "10006", ItemName = "Steel Wire 500m", Subcategory = "Building Materials", Unit = "ROLL", CostPrice = 85.00m, SellingPrice = 105.00m, LabelPrice = 110.00m, AvailableQuantity = 22 },
                new MockItem { ItemCode = "10007", ItemName = "Ladder 6ft", Subcategory = "Access Equipment", Unit = "PCS", CostPrice = 45.00m, SellingPrice = 55.00m, LabelPrice = 60.00m, AvailableQuantity = 12 },
                new MockItem { ItemCode = "10008", ItemName = "Paint Brush Set", Subcategory = "Painting Tools", Unit = "SET", CostPrice = 12.00m, SellingPrice = 18.50m, LabelPrice = 20.00m, AvailableQuantity = 65 }
            };

            return new ObservableCollection<MockItem>(mockDatabase
                .Where(item =>
                    item.ItemCode.ToLower().Contains(searchTerm) ||
                    item.ItemName.ToLower().Contains(searchTerm) ||
                    item.Subcategory.ToLower().Contains(searchTerm))
                .Take(10));
        }

        private void TxtItemName_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isSearchMode || !_searchResults.Any()) return;

            if (e.Key == Key.Down)
            {
                e.Handled = true;
                _selectedIndex = Math.Min(_selectedIndex + 1, _searchResults.Count - 1);
                dgItems.SelectedIndex = _selectedIndex;
                dgItems.ScrollIntoView(dgItems.SelectedItem);
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                _selectedIndex = Math.Max(_selectedIndex - 1, 0);
                dgItems.SelectedIndex = _selectedIndex;
                dgItems.ScrollIntoView(dgItems.SelectedItem);
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (_selectedIndex >= 0 && _searchResults.Count > _selectedIndex)
                {
                    SelectItemFromSearch(_searchResults[_selectedIndex]);
                }
            }
        }

        private void DgItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgItems.SelectedItem is ItemViewModel selectedItem)
            {
                if (_isSearchMode && selectedItem.IsSearchResult)
                {
                    SelectItemFromSearch(selectedItem);
                }
            }
        }

        private void SelectItemFromSearch(ItemViewModel item)
        {
            if (item.OriginalItem == null) return;

            txtItemCode.Text = item.ItemCode;
            txtItemName.Text = item.ItemName;
            txtSubcategory.Text = item.Subcategory;
            txtUnit.Text = item.Unit;
            txtCostPrice.Text = item.CostPrice.ToString("F2");
            txtSellingPrice.Text = item.SellingPrice.ToString("F2");
            txtLabelPrice.Text = item.LabelPrice.ToString("F2");

            _isSearchMode = false;
            dgItems.ItemsSource = Items;
            txtSearchInfo.Visibility = Visibility.Collapsed;

            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }

        private void TxtCostPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Slash function: Calculate cost price from total price divided by quantity
            var text = txtCostPrice.Text;

            if (text.EndsWith("/"))
            {
                try
                {
                    var totalPriceText = text.Replace("/", "");
                    if (decimal.TryParse(totalPriceText, out decimal totalPrice))
                    {
                        if (int.TryParse(txtQuantity.Text, out int quantity) && quantity > 0)
                        {
                            var costPrice = totalPrice / quantity;
                            txtCostPrice.Text = costPrice.ToString("F2");
                        }
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }
        }

        private void TxtSellingPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Asterisk function: Calculate selling price from percentage discount on label price
            var text = txtSellingPrice.Text;

            if (text.Contains("*"))
            {
                try
                {
                    var percentageText = text.Replace("*", "");
                    if (decimal.TryParse(percentageText, out decimal percentage))
                    {
                        if (decimal.TryParse(txtLabelPrice.Text, out decimal labelPrice))
                        {
                            var discount = labelPrice * (percentage / 100);
                            var sellingPrice = labelPrice - discount;
                            txtSellingPrice.Text = sellingPrice.ToString("F2");
                        }
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var newItem = new ItemViewModel
            {
                Id = Guid.NewGuid().ToString(),
                ItemCode = txtItemCode.Text,
                ItemName = txtItemName.Text,
                Subcategory = txtSubcategory.Text,
                Unit = txtUnit.Text,
                Quantity = int.Parse(txtQuantity.Text),
                CostPrice = decimal.Parse(txtCostPrice.Text),
                SellingPrice = decimal.Parse(txtSellingPrice.Text),
                LabelPrice = decimal.Parse(txtLabelPrice.Text),
                AvailableQuantity = 0,
                IsSearchResult = false
            };

            Items.Add(newItem);
            ClearInputFields();
            UpdateSummary();
            txtItemName.Focus();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtItemCode.Text))
            {
                MessageBox.Show("Please enter Item Code", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Please enter Item Name", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter valid quantity", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(txtCostPrice.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Please enter valid cost price", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearInputFields()
        {
            txtItemCode.Clear();
            txtItemName.Clear();
            txtSubcategory.Clear();
            txtUnit.Text = "PCS";
            txtQuantity.Text = "1";
            txtCostPrice.Clear();
            txtSellingPrice.Clear();
            txtLabelPrice.Clear();
        }

        private void UpdateSummary()
        {
            txtItemsAdded.Text = Items.Count.ToString();

            decimal totalAmount = Items.Sum(item => item.Quantity * item.CostPrice);
            txtTotal.Text = $"${totalAmount:F2}";
        }

        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ItemViewModel item)
            {
                Items.Remove(item);
                UpdateSummary();
            }
        }

        /*private void BtnPrintBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ItemViewModel item)
            {
                var printWindow = new PrintBarcodeWindow(item);
                printWindow.Owner = this;
                printWindow.ShowDialog();
            }
        }*/

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Items.Count == 0)
            {
                MessageBox.Show("Please add at least one item", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Please select a supplier", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Here you would save the items and return to parent window
            MessageBox.Show($"Saved {Items.Count} items successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ItemViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _itemCode;
        private string _itemName;
        private string _subcategory;
        private string _unit;
        private int _quantity;
        private decimal _costPrice;
        private decimal _sellingPrice;
        private decimal _labelPrice;
        private int _availableQuantity;
        private bool _isSearchResult;
        private MockItem _originalItem;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string ItemCode
        {
            get => _itemCode;
            set { _itemCode = value; OnPropertyChanged(nameof(ItemCode)); }
        }

        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(nameof(ItemName)); }
        }

        public string Subcategory
        {
            get => _subcategory;
            set { _subcategory = value; OnPropertyChanged(nameof(Subcategory)); }
        }

        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(nameof(Unit)); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(Amount)); }
        }

        public decimal CostPrice
        {
            get => _costPrice;
            set { _costPrice = value; OnPropertyChanged(nameof(CostPrice)); OnPropertyChanged(nameof(Amount)); }
        }

        public decimal SellingPrice
        {
            get => _sellingPrice;
            set { _sellingPrice = value; OnPropertyChanged(nameof(SellingPrice)); }
        }

        public decimal LabelPrice
        {
            get => _labelPrice;
            set { _labelPrice = value; OnPropertyChanged(nameof(LabelPrice)); }
        }

        public int AvailableQuantity
        {
            get => _availableQuantity;
            set { _availableQuantity = value; OnPropertyChanged(nameof(AvailableQuantity)); }
        }

        public bool IsSearchResult
        {
            get => _isSearchResult;
            set { _isSearchResult = value; OnPropertyChanged(nameof(IsSearchResult)); }
        }

        public MockItem OriginalItem
        {
            get => _originalItem;
            set { _originalItem = value; OnPropertyChanged(nameof(OriginalItem)); }
        }

        public decimal Amount => Quantity * CostPrice;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SupplierViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _name;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MockItem
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Subcategory { get; set; }
        public string Unit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal LabelPrice { get; set; }
        public int AvailableQuantity { get; set; }
    }

    // Converter for Inverted Boolean
    public class BooleanToNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}