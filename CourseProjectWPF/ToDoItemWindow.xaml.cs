using System;
using System.Windows;
using ClassLibrary.Classes;

namespace CourseProjectWPF
{
    public partial class ToDoItemWindow : Window
    {
        public ToDoItem Item { get; }
        public bool ToDelete { get; private set; }

        public ToDoItemWindow()
        {
            InitializeComponent();

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
            Item                            = new ToDoItem();
            ToDelete                        = false;
        }

        public void ShowDialog(DateTime dateTime)
        {
            PickedDate.SelectedDate = dateTime;

            ShowDialog();
        }

        public ToDoItemWindow(ToDoItem item)
        {
            InitializeComponent();

            Item = new ToDoItem {Id = item.Id};

            HeaderText.Text = item.Header;
            NotesText.Text  = item.Notes;

            if (item.Date != DateTime.MinValue)
                PickedDate.SelectedDate = item.Date;

            if (item.Deadline != DateTime.MinValue)
                PickedDeadline.SelectedDate = item.Deadline;

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HeaderText.Text))
            {
                MessageBox.Show("You need to fill header");
                return;
            }

            Item.Header = HeaderText.Text;
            Item.Notes  = NotesText.Text;

            if (PickedDate.SelectedDate != null)
                Item.Date = (DateTime) PickedDate.SelectedDate;

            if (PickedDeadline.SelectedDate != null)
                Item.Deadline = (DateTime) PickedDeadline.SelectedDate;

            DialogResult = true;
            Close();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            ToDelete = true;
            Close();
        }
    }
}