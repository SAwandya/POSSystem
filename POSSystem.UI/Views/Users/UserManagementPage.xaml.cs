using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace POSSystem.UI.Views.Users
{
    public partial class UserManagementPage : Window
    {
        private readonly IUserService _userService;
        private ObservableCollection<UserDto> _users;

        public UserManagementPage()
        {
            InitializeComponent();

            // Get service from DI
            var app = (App)System.Windows.Application.Current;
            _userService = app.ServiceProvider.GetRequiredService<IUserService>();

            _users = new ObservableCollection<UserDto>();
            dgUsers.ItemsSource = _users;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _users.Clear();
                foreach (var user in users)
                {
                    _users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search users by name or username...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search users by name or username...";
                txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 208));
            }
        }

        private async void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text == "Search users by name or username...") return;

            var searchTerm = txtSearch.Text.ToLower();
            var allUsers = await _userService.GetAllUsersAsync();

            _users.Clear();
            foreach (var user in allUsers.Where(u =>
                u.Username.ToLower().Contains(searchTerm) ||
                (u.FullName != null && u.FullName.ToLower().Contains(searchTerm))))
            {
                _users.Add(user);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditUserDialog(_userService);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                _ = LoadUsersAsync();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var dialog = new AddEditUserDialog(_userService, userId);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    _ = LoadUsersAsync();
                }
            }
        }

        private async void ToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                var action = user.IsActive ? "deactivate" : "activate";
                var result = MessageBox.Show(
                    $"Are you sure you want to {action} user '{user.Username}'?",
                    "Confirm Action",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var toggleResult = await _userService.ToggleUserStatusAsync(userId);
                    if (toggleResult.Success)
                    {
                        MessageBox.Show(toggleResult.Message, "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsersAsync();
                    }
                    else
                    {
                        MessageBox.Show(toggleResult.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ManagePermissions_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _users.FirstOrDefault(u => u.UserId == userId);
                if (user == null) return;

                var dialog = new ManagePermissionsDialog(_userService, userId, user.Username);
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
