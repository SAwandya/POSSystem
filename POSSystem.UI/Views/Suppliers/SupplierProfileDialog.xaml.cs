using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Windows;

namespace POSSystem.UI.Views.Suppliers
{
    public partial class SupplierProfileDialog : Window
    {
        private readonly Supplier _supplier;
        private readonly IUnitOfWork _unitOfWork;
        private bool _isEditMode = false;

        // Store original values for cancel functionality
        private string _originalName = string.Empty;
        private string? _originalContactPerson;
        private string? _originalPhone;
        private string? _originalEmail;
        private string? _originalAddress;

        public SupplierProfileDialog(Supplier supplier, IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _supplier = supplier;
            _unitOfWork = unitOfWork;

            LoadSupplierData();
        }

        private void LoadSupplierData()
        {
            // Store original values
            _originalName = _supplier.Name;
            _originalContactPerson = _supplier.ContactPerson;
            _originalPhone = _supplier.Phone;
            _originalEmail = _supplier.Email;
            _originalAddress = _supplier.Address;

            // Display data
            txtSupplierId.Text = $"SUP-{_supplier.SupplierId:D4}";
            
            // View mode fields
            txtSupplierNameView.Text = _supplier.Name;
            txtContactPersonView.Text = _supplier.ContactPerson ?? "N/A";
            txtPhoneView.Text = _supplier.Phone ?? "N/A";
            txtEmailView.Text = _supplier.Email ?? "N/A";
            txtAddressView.Text = _supplier.Address ?? "N/A";

            // Edit mode fields
            txtSupplierNameEdit.Text = _supplier.Name;
            txtContactPersonEdit.Text = _supplier.ContactPerson ?? string.Empty;
            txtPhoneEdit.Text = _supplier.Phone ?? string.Empty;
            txtEmailEdit.Text = _supplier.Email ?? string.Empty;
            txtAddressEdit.Text = _supplier.Address ?? string.Empty;

            // Load statistics
            LoadStatistics();
        }

        private async void LoadStatistics()
        {
            try
            {
                // Count GRNs for this supplier
                var grns = await _unitOfWork.Repository<POSSystem.Domain.Entities.GRN>().GetAllAsync();
                var supplierGRNs = grns.Where(g => g.SupplierId == _supplier.SupplierId).ToList();

                txtTotalGRNs.Text = supplierGRNs.Count.ToString();

                decimal totalAmount = supplierGRNs.Sum(g => g.TotalAmount ?? 0);
                txtTotalAmount.Text = $"Rs {totalAmount:#,##0.00}";
            }
            catch
            {
                txtTotalGRNs.Text = "0";
                txtTotalAmount.Text = "Rs 0.00";
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch to edit mode
            _isEditMode = true;
            
            // Update UI
            txtModeIndicator.Text = "?? Edit Mode - Modify supplier details";
            
            // Hide view mode controls
            txtSupplierNameView.Visibility = Visibility.Collapsed;
            txtContactPersonView.Visibility = Visibility.Collapsed;
            txtPhoneView.Visibility = Visibility.Collapsed;
            txtEmailView.Visibility = Visibility.Collapsed;
            txtAddressView.Visibility = Visibility.Collapsed;

            // Show edit mode controls
            txtSupplierNameEdit.Visibility = Visibility.Visible;
            txtContactPersonEdit.Visibility = Visibility.Visible;
            txtPhoneEdit.Visibility = Visibility.Visible;
            txtEmailEdit.Visibility = Visibility.Visible;
            txtAddressEdit.Visibility = Visibility.Visible;

            // Switch button panels
            ViewModeButtons.Visibility = Visibility.Collapsed;
            EditModeButtons.Visibility = Visibility.Visible;

            // Focus on first editable field
            txtSupplierNameEdit.Focus();
            txtSupplierNameEdit.SelectAll();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtSupplierNameEdit.Text))
            {
                MessageBox.Show("Supplier name is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Save changes to this supplier?",
                "Confirm Save",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Update supplier object
                    _supplier.Name = txtSupplierNameEdit.Text.Trim();
                    _supplier.ContactPerson = string.IsNullOrWhiteSpace(txtContactPersonEdit.Text) 
                        ? null : txtContactPersonEdit.Text.Trim();
                    _supplier.Phone = string.IsNullOrWhiteSpace(txtPhoneEdit.Text) 
                        ? null : txtPhoneEdit.Text.Trim();
                    _supplier.Email = string.IsNullOrWhiteSpace(txtEmailEdit.Text) 
                        ? null : txtEmailEdit.Text.Trim();
                    _supplier.Address = string.IsNullOrWhiteSpace(txtAddressEdit.Text) 
                        ? null : txtAddressEdit.Text.Trim();

                    // Save to database
                    _unitOfWork.Repository<Supplier>().Update(_supplier);
                    await _unitOfWork.SaveChangesAsync();

                    // Update original values
                    _originalName = _supplier.Name;
                    _originalContactPerson = _supplier.ContactPerson;
                    _originalPhone = _supplier.Phone;
                    _originalEmail = _supplier.Email;
                    _originalAddress = _supplier.Address;

                    MessageBox.Show("Supplier updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Switch back to view mode
                    SwitchToViewMode();

                    // Set dialog result to refresh parent window
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving supplier:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Discard all changes?",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Restore original values to edit controls
                txtSupplierNameEdit.Text = _originalName;
                txtContactPersonEdit.Text = _originalContactPerson ?? string.Empty;
                txtPhoneEdit.Text = _originalPhone ?? string.Empty;
                txtEmailEdit.Text = _originalEmail ?? string.Empty;
                txtAddressEdit.Text = _originalAddress ?? string.Empty;

                // Restore to view mode
                SwitchToViewMode();
            }
        }

        private void SwitchToViewMode()
        {
            _isEditMode = false;

            // Update UI
            txtModeIndicator.Text = "View and manage supplier details";

            // Update view mode controls with current values
            txtSupplierNameView.Text = txtSupplierNameEdit.Text;
            txtContactPersonView.Text = string.IsNullOrWhiteSpace(txtContactPersonEdit.Text) 
                ? "N/A" : txtContactPersonEdit.Text;
            txtPhoneView.Text = string.IsNullOrWhiteSpace(txtPhoneEdit.Text) 
                ? "N/A" : txtPhoneEdit.Text;
            txtEmailView.Text = string.IsNullOrWhiteSpace(txtEmailEdit.Text) 
                ? "N/A" : txtEmailEdit.Text;
            txtAddressView.Text = string.IsNullOrWhiteSpace(txtAddressEdit.Text) 
                ? "N/A" : txtAddressEdit.Text;

            // Show view mode controls
            txtSupplierNameView.Visibility = Visibility.Visible;
            txtContactPersonView.Visibility = Visibility.Visible;
            txtPhoneView.Visibility = Visibility.Visible;
            txtEmailView.Visibility = Visibility.Visible;
            txtAddressView.Visibility = Visibility.Visible;

            // Hide edit mode controls
            txtSupplierNameEdit.Visibility = Visibility.Collapsed;
            txtContactPersonEdit.Visibility = Visibility.Collapsed;
            txtPhoneEdit.Visibility = Visibility.Collapsed;
            txtEmailEdit.Visibility = Visibility.Collapsed;
            txtAddressEdit.Visibility = Visibility.Collapsed;

            // Switch button panels
            ViewModeButtons.Visibility = Visibility.Visible;
            EditModeButtons.Visibility = Visibility.Collapsed;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete supplier '{_supplier.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Check if supplier has any GRNs
                    var grns = await _unitOfWork.Repository<POSSystem.Domain.Entities.GRN>().GetAllAsync();
                    var hasGRNs = grns.Any(g => g.SupplierId == _supplier.SupplierId);

                    if (hasGRNs)
                    {
                        MessageBox.Show(
                            "Cannot delete this supplier because it has associated GRN records.\n\nPlease remove or reassign the GRNs first.",
                            "Cannot Delete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var supplierRepo = _unitOfWork.Repository<Supplier>();
                    var supplierToDelete = await supplierRepo.GetByIdAsync(_supplier.SupplierId);
                    if (supplierToDelete != null)
                    {
                        supplierRepo.Remove(supplierToDelete);
                        await _unitOfWork.SaveChangesAsync();

                        MessageBox.Show("Supplier deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        DialogResult = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting supplier:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Close anyway?",
                    "Confirm Close",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

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
}
