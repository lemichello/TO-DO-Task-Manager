using System.Windows;

namespace ToDoTaskManager
{
    public partial class TagWindow : Window
    {
        public string NewText { get; private set; }

        public TagWindow()
        {
            InitializeComponent();
        }

        private void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TagTextBox.Text))
            {
                MessageBox.Show("You need to fill tag text field");
                return;
            }

            DialogResult = true;
            NewText      = TagTextBox.Text;

            Close();
        }
    }
}