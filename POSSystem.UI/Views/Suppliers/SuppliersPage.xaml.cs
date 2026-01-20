using Microsoft.Extensions.DependencyInjection;
using POSSystem.Domain.Entities;
using POSSystem.Infrastructure.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSystem.UI.Views.Suppliers
{
    public partial class SuppliersPage : Window
    {
        private readonly IUnitOfWork _unitOfWork;
        public ObservableCollection<SupplierViewModel> AllSuppliers { get; set; }
        public ObservableCollection<SupplierViewModel> FilteredSuppliers { get; set; }

        public SuppliersPage()
        {
            // Initialize collections FIRST
            AllSuppliers = new ObservableCollection<SupplierViewModel>();
            FilteredSuppliers = new ObservableCollection<SupplierViewModel>();

            InitializeComponent();

            // Get service from DI container
            var app = (App)System.Windows.Application.Current;
            _unitOfWork = app.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Set DataContext
            DataContext = this;

            // Load data when window is fully loaded
            this.Loaded += SuppliersPage_Loaded;
        }

        private void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliersAsync();
        }

        private async void LoadSuppliersAsync()
        {
            try
            {
                // Show loading
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Visible;

                // Fetch all suppliers
                var suppliers = await _unitOfWork.Repository<Supplier>().GetAllAsync();

                AllSuppliers.Clear();
                FilteredSuppliers.Clear();

                foreach (var supplier in suppliers)
                {
                    var item = new SupplierViewModel
                    {
                        SupplierId = supplier.SupplierId,
                        Name = supplier.Name,
                        ContactPerson = supplier.ContactPerson ?? "N/A",
                        Phone = supplier.Phone ?? "N/A",
                        Email = supplier.Email ?? "N/A",
                        Address = supplier.Address ?? "N/A"
                    };

                    AllSuppliers.Add(item);
                    FilteredSuppliers.Add(item);
                }

                // Bind to DataGrid
                if (SuppliersDataGrid != null)
                {
                    SuppliersDataGrid.ItemsSource = FilteredSuppliers;
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
                MessageBox.Show($"Error loading suppliers: {ex.Message}\n\nInner Exception: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateStatistics()
        {
            int totalSuppliers = AllSuppliers.Count;

            if (SupplierCountText != null)
                SupplierCountText.Text = $"{totalSuppliers} supplier{(totalSuppliers != 1 ? "s" : "")}";

            if (RecordCountText != null)
                RecordCountText.Text = $"{FilteredSuppliers.Count} of {totalSuppliers} suppliers displayed";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox == null || searchBox.Text == "Search suppliers by name, contact person, or phone...")
            {
                FilteredSuppliers.Clear();
                foreach (var supplier in AllSuppliers)
                {
                    FilteredSuppliers.Add(supplier);
                }
                UpdateStatistics();
                return;
            }

            string searchTerm = searchBox.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                FilteredSuppliers.Clear();
                foreach (var supplier in AllSuppliers)
                {
                    FilteredSuppliers.Add(supplier);
                }
                UpdateStatistics();
                return;
            }

            FilteredSuppliers.Clear();

            foreach (var supplier in AllSuppliers)
            {
                bool matches = supplier.Name.ToLower().Contains(searchTerm) ||
                              (supplier.ContactPerson?.ToLower().Contains(searchTerm) ?? false) ||
                              (supplier.Phone?.ToLower().Contains(searchTerm) ?? false);

                if (matches)
                {
                    FilteredSuppliers.Add(supplier);
                }
            }

            UpdateStatistics();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Search suppliers by name, contact person, or phone...")
            {
                textBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Search suppliers by name, contact person, or phone...";
            }
        }

        private void AddNewSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddSupplierDialog(_unitOfWork);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                LoadSuppliersAsync();
            }
        }

        private void SuppliersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SuppliersDataGrid.SelectedItem is SupplierViewModel selectedSupplier)
            {
                ShowSupplierProfile(selectedSupplier.SupplierId);
            }
        }

        private async void ShowSupplierProfile(int supplierId)
        {
            try
            {
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(supplierId);
                if (supplier != null)
                {
                    var profileDialog = new SupplierProfileDialog(supplier, _unitOfWork);
                    profileDialog.Owner = this;

                    if (profileDialog.ShowDialog() == true)
                    {
                        LoadSuppliersAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading supplier profile: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSuppliersAsync();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to Excel/PDF functionality will be implemented here.",
                "Export Suppliers", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // ViewModel for Supplier Items
    public class SupplierViewModel
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    }
}
