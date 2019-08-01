using System.Windows;
using BUS.Services;

namespace ToDoTaskManager
{
    public partial class RegistrationWindow : Window
    {
        private readonly UserService _service;
        
        public RegistrationWindow(UserService service)
        {
            InitializeComponent();

            _service = service;
        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(RepeatPasswordBox.Password))
            {
                MessageBox.Show("You need to fill all fields.");
                return;
            }

            if (LoginTextBox.Text.Contains(" ") ||
                PasswordBox.Password.Contains(" "))
            {
                MessageBox.Show("Spaces are not allowed in login and password. " +
                                "Instead use \'_\' symbol");
                return;
            }
            
            if (PasswordBox.Password != RepeatPasswordBox.Password)
            {
                MessageBox.Show("Passwords don\'t match");
                return;
            }

            if(_service.Add(LoginTextBox.Text, PasswordBox.Password))
            {
                MessageBox.Show("Successfully registered.");
                Close();
            }
        }
    }
}