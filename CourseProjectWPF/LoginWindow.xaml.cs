using System.Windows;
using BUS.Services;

namespace CourseProjectWPF
{
    public partial class LoginWindow : Window
    {
        private readonly UserService _service;

        public LoginWindow()
        {
            InitializeComponent();

            _service = new UserService();
        }

        private void EnterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("You need to fill login and password fields");
                return;
            }

            var id = _service.GetUserId(LoginTextBox.Text, PasswordBox.Password);

            if (id == -1)
            {
                MessageBox.Show("You entered incorrect login or password. Try again.");
                return;
            }

            var window = new MainWindow(this, id);

            window.Show();
            Hide();

            LoginTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow(_service);

            window.ShowDialog();
        }
    }
}