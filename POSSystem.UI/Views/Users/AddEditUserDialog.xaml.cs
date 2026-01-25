using POSSystem.Application.Services;
using POSSystem.Domain.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace POSSystem.UI.Views.Users
{
    public partial class AddEditUserDialog : Window
    {
        private readonly IUserService _userService;
        private readonly int? _userId;
        private readonly bool _isEditMode;

        public AddEditUserDialog(IUserService userService, int? userId = null)
        {
            InitializeComponent();
            _userService = userService;
            _userId = userId;
            _isEditMode = userId.HasValue;

            InitializeDialog();
        }

        private async void InitializeDialog()
        {
            // Setup role combobox
            cmbRole.ItemsSource = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            cmbRole.SelectedIndex = 0;

            if (_isEditMode && _userId.HasValue)
            {
                txtTitle.Text = "Edit User";
                txtPassword.Visibility = Visibility.Collapsed;
                
                // Find and hide password label by searching parent
                var parent = txtPassword.Parent as System.Windows.Controls.Panel;
                if (parent != null)
                {
                    int passwordIndex = parent.Children.IndexOf(txtPassword);
                    if (passwordIndex > 0)
                    {
                        var previousElement = parent.Children[passwordIndex - 1];
                        if (previousElement is System.Windows.Controls.TextBlock tb && tb.Text == "Password")
                        {
                            tb.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                var user = await _userService.GetUserByIdAsync(_userId.Value);
                if (user != null)
                {
                    txtUsername.Text = user.Username;
                    txtUsername.IsEnabled = false; // Don't allow username change
                    txtFullName.Text = user.FullName ?? "";
                    cmbRole.SelectedItem = user.Role;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Username is required", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Password is required", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEditMode && _userId.HasValue)
                {
                    var updateDto = new UpdateUserDto
                    {
                        FullName = txtFullName.Text,
                        Role = (UserRole)cmbRole.SelectedItem
                    };

                    var result = await _userService.UpdateUserAsync(_userId.Value, updateDto);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var createDto = new CreateUserDto
                    {
                        Username = txtUsername.Text,
                        Password = txtPassword.Password,
                        FullName = txtFullName.Text,
                        Role = (UserRole)cmbRole.SelectedItem
                    };

                    var result = await _userService.CreateUserAsync(createDto);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
