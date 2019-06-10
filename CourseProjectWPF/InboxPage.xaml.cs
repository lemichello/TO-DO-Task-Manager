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
    /// Interaction logic for InboxPage.xaml
    /// </summary>
    public partial class InboxPage : Page
    {
        private readonly ObservableCollection<ToDoItem> _toDoItemsCollection;
        private readonly string                         _connectionString;
        private readonly InboxToDoItemOperations        _toDoItemOperations;

        public InboxPage()
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItem>();

            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
            _toDoItemOperations           = new InboxToDoItemOperations(ToDoItemsListView, _toDoItemsCollection);
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            // TODO: Add to Logbook
            _toDoItemOperations.Checked(sender);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _toDoItemOperations.Selected();
        }

        private void FillCollection()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "SELECT * FROM ToDoItems WHERE Date=''";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        MainWindow.FillCollection(reader, _toDoItemsCollection);
                    }
                }
            }
        }
    }
}