using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Windows;

namespace POSSystem.UI.Views.Suppliers
{
    public partial class AddSupplierDialog : Window
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddSupplierDialog(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
            {
                MessageBox.Show("Please enter supplier name", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var supplier = new Supplier
                {
                    Name = txtSupplierName.Text.Trim(),
                    ContactPerson = string.IsNullOrWhiteSpace(txtContactPerson.Text) ? null : txtContactPerson.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim()
                };

                await _unitOfWork.Repository<Supplier>().AddAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                MessageBox.Show("Supplier added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier:\n\n{ex.Message}\n\nInner Exception:\n{ex.InnerException?.Message}",
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
