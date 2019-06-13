using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using ClassLibrary.Classes;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for TodayPage.xaml
    /// </summary>
    public partial class TodayPage : Page
    {
        private readonly ObservableCollection<ToDoItem> _toDoItemsCollection;
        private readonly string                         _connectionString;
        private readonly TodayToDoItemOperations        _toDoItemOperations;

        public TodayPage()
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItem>();

            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
            _toDoItemOperations           = new TodayToDoItemOperations(ToDoItemsListView, _toDoItemsCollection);
        }

        private void FillCollection()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "SELECT * FROM ToDoItems WHERE Date<=@todayDate";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@todayDate",
                        ((long) (DateTime.Today - DateTime.MinValue).TotalMilliseconds).ToString());

                    using (var reader = cmd.ExecuteReader())
                    {
                        DatabaseOperations.FillCollection(reader, _toDoItemsCollection);
                    }
                }
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _toDoItemOperations.Selected();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Checked(sender);
        }
    }
}