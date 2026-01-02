using POSSystem.UI.Views.Dashboard;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace POSSystem.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            // Enable dragging
            this.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Animate the login form
            AnimateLoginForm();

            // Focus on username box
            UsernameBox.Focus();
        }

        private void AnimateLoginForm()
        {
            // Create fade-in animation
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Apply animation to login form
            LoginButton.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            this.Close();

           
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ShowStatusMessage("Password reset link sent to your email", false);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Exit POS Pro",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void ShowStatusMessage(string message, bool isError)
        {
            StatusText.Text = message;
            StatusMessage.Visibility = Visibility.Visible;

            // Auto hide after 3 seconds
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                StatusMessage.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        // Handle Enter key in password box
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }
    }
}