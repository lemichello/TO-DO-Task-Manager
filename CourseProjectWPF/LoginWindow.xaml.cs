using System.Configuration;
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
            Hide();

            _service = new UserService();
            
            IsUserLoggedIn();
        }

        private void IsUserLoggedIn()
        {
            var loginStatus = ConfigurationManager.AppSettings["UserId"];

            if (loginStatus == null || loginStatus == "-1")
            {
                Show();
                return;
            }
            
            var window = new MainWindow(this, int.Parse(loginStatus));
            
            window.Show();
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
            
            SetLoginInfo(id);
        }

        private static void SetLoginInfo(int id)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); 
            
            config.AppSettings.Settings["UserId"].Value = id.ToString();
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        
        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow(_service);

            window.ShowDialog();
        }
    }
}