using POSSystem.Application.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace POSSystem.UI.Views.Users
{
    public partial class ManagePermissionsDialog : Window
    {
        private readonly IUserService _userService;
        private readonly int _userId;
        private ObservableCollection<PermissionDto> _permissions;

        public ManagePermissionsDialog(IUserService userService, int userId, string username)
        {
            InitializeComponent();
            _userService = userService;
            _userId = userId;
            _permissions = new ObservableCollection<PermissionDto>();
            
            txtUsername.Text = $"User: {username}";
            PermissionsList.ItemsSource = _permissions;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var permissions = await _userService.GetUserPermissionsAsync(_userId);
                
                _permissions.Clear();
                foreach (var permission in permissions.OrderBy(p => p.ModuleGroup).ThenBy(p => p.Name))
                {
                    _permissions.Add(permission);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading permissions: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var grantedPermissions = _permissions.Where(p => p.IsGranted).Select(p => p.PermissionId).ToList();
                
                var result = await _userService.UpdateUserPermissionsAsync(_userId, grantedPermissions);
                
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving permissions: {ex.Message}", "Error",
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
