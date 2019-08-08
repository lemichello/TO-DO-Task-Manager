using System.Configuration;
using System.Windows;
using BUS.Services;

namespace ToDoTaskManager
{
    public partial class LoginWindow
    {
        private readonly UserService _service;

        public LoginWindow()
        {
            InitializeComponent();
            Hide();

            _service = UserService.GetInstance();

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

            InitializeServices(int.Parse(loginStatus));
            
            var window = new MainWindow(this);

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

            InitializeServices(id);

            var window = new MainWindow(this);

            window.Show();
            Hide();

            LoginTextBox.Text    = string.Empty;
            PasswordBox.Password = string.Empty;

            // User chose to not remember his password. 
            if (RememberPasswordCheckBox?.IsChecked != null &&
                !(bool) RememberPasswordCheckBox.IsChecked)
                return;

            SetLoginInfo(id);

            if (RememberPasswordCheckBox != null)
                RememberPasswordCheckBox.IsChecked = false;
        }

        private static void InitializeServices(int id)
        {
            TagService.Initialize(id);
            ProjectService.Initialize(id);
            ToDoItemService.Initialize(id);
        }

        public void Refresh()
        {
            _service.RefreshRepositories();
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