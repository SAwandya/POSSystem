using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace POSSystem.UI.Views.GRN
{
    public partial class GRNPage : Window
    {
        private ObservableCollection<GRNItem> _grnItems = new ObservableCollection<GRNItem>();
        private int _itemCounter = 1;

        public GRNPage()
        {
            InitializeComponent();

            // Set up DataGrid data source
            dgGRNItems.ItemsSource = _grnItems;

            // Set default dates
            dpGRNDate.SelectedDate = DateTime.Now;
            dpDeliveryDate.SelectedDate = DateTime.Now;

            // Update UI
            UpdateItemCount();
        }

        #region Event Handlers

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to go back? Unsaved changes will be lost.",
                "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality would be implemented here.",
                "Print", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveDraftButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateGRN())
            {
                MessageBox.Show("GRN draft saved successfully!",
                    "Save Draft", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EmailGRNButton_Click(object sender, RoutedEventArgs e)
        {
            if (_grnItems.Count == 0)
            {
                MessageBox.Show("Please add items before emailing GRN.",
                    "Email GRN", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Email GRN functionality would be implemented here.",
                "Email GRN", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (_grnItems.Count == 0)
            {
                MessageBox.Show("No items to update in stock.",
                    "Update Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Stock updated successfully!",
                "Update Stock", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset form
            txtPONumber.Clear();
            txtInvoiceNumber.Clear();
            txtReceivedBy.Clear();
            txtNotes.Clear();
            txtQuantity.Text = "1";
            txtUnitPrice.Text = "0.00";
            txtBatchNo.Clear();
            txtLocation.Text = "Warehouse A";

            // Reset comboboxes
            cmbSupplier.SelectedIndex = 0;
            cmbItem.SelectedIndex = 0;

            // Reset dates
            dpGRNDate.SelectedDate = DateTime.Now;
            dpDeliveryDate.SelectedDate = DateTime.Now;
            dpExpiryDate.SelectedDate = null;

            MessageBox.Show("Form refreshed successfully!",
                "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgGRNItems.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to edit.",
                    "Edit Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = (GRNItem)dgGRNItems.SelectedItem;
            MessageBox.Show($"Editing item: {selectedItem.ItemName}",
                "Edit Item", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgGRNItems.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to remove.",
                    "Remove Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to remove this item?",
                "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _grnItems.Remove((GRNItem)dgGRNItems.SelectedItem);
                UpdateItemCount();
                CalculateTotals();
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_grnItems.Count == 0)
            {
                MessageBox.Show("No items to clear.",
                    "Clear All", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to clear all items?",
                "Confirm Clear All", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _grnItems.Clear();
                _itemCounter = 1;
                UpdateItemCount();
                CalculateTotals();
            }
        }

        private void CompleteGRNButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateGRN())
            {
                return;
            }

            if (_grnItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item before completing GRN.",
                    "Complete GRN", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to complete this GRN? This will finalize the document.",
                "Confirm Complete GRN", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Generate new GRN number
                string newGRN = $"GRN-{DateTime.Now:yyyy}-{new Random().Next(10000, 99999)}";
                txtGRNNumber.Text = newGRN;

                MessageBox.Show($"GRN {newGRN} completed successfully!",
                    "Complete GRN", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset for next GRN
                _grnItems.Clear();
                _itemCounter = 1;
                UpdateItemCount();
                CalculateTotals();
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            // Validate item selection
            if (cmbItem.SelectedIndex == 0)
            {
                MessageBox.Show("Please select an item.",
                    "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate quantity
            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.",
                    "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate price
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Please enter a valid unit price.",
                    "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get item name from ComboBox
            string? itemName = ((ComboBoxItem)cmbItem.SelectedItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(itemName))
            {
                MessageBox.Show("Invalid item selected.",
                    "Add Item", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create new item with initialized properties
            var newItem = new GRNItem
            {
                SNo = _itemCounter++,
                ItemName = itemName,
                SKU = $"SKU-{new Random().Next(1000, 9999)}",
                Quantity = quantity,
                Unit = "pcs", // Provide default value
                UnitPrice = unitPrice,
                BatchNo = string.IsNullOrWhiteSpace(txtBatchNo.Text) ? $"BATCH-{DateTime.Now:yyMMdd}" : txtBatchNo.Text,
                ExpiryDate = dpExpiryDate.SelectedDate,
                Location = txtLocation.Text ?? "Warehouse A" // Provide default value
            };

            _grnItems.Add(newItem);

            // Update UI
            UpdateItemCount();
            CalculateTotals();

            // Reset form for next item
            cmbItem.SelectedIndex = 0;
            txtQuantity.Text = "1";
            txtUnitPrice.Text = "0.00";
            txtBatchNo.Clear();
            dpExpiryDate.SelectedDate = null;

            // Auto-scroll to new item
            dgGRNItems.ScrollIntoView(newItem);
            dgGRNItems.SelectedItem = newItem;
        }

        #endregion

        #region Helper Methods

        private bool ValidateGRN()
        {
            if (cmbSupplier.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a supplier.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtReceivedBy.Text))
            {
                MessageBox.Show("Please enter the name of the person who received the goods.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (dpGRNDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a GRN date.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void UpdateItemCount()
        {
            txtItemCount.Text = $"{_grnItems.Count} items";
            txtFooterItemCount.Text = _grnItems.Count.ToString();
        }

        private void CalculateTotals()
        {
            decimal subtotal = 0;

            foreach (GRNItem item in _grnItems)
            {
                subtotal += item.Total;
            }

            decimal tax = subtotal * 0.10m;
            decimal grandTotal = subtotal + tax;

            txtSubtotal.Text = subtotal.ToString("C");
            txtTax.Text = tax.ToString("C");
            txtGrandTotal.Text = grandTotal.ToString("C");
        }

        #endregion
    }

    #region Data Models

    public class GRNItem
    {
        // Initialize properties with default values to avoid null warnings
        public int SNo { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public string Unit { get; set; } = "pcs"; // Default value
        public decimal UnitPrice { get; set; } = 0;
        public decimal Total => Quantity * UnitPrice;
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string Location { get; set; } = "Warehouse A"; // Default value
    }

    #endregion
}