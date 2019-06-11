using ClassLibrary.Classes;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows.Controls;

namespace CourseProjectWPF
{
    public partial class LogbookPage : Page
    {
        private readonly ObservableCollection<ToDoItem> _toDoItemsCollection;
        private readonly string _connectionString;

        public LogbookPage()
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItem>();
            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void FillCollection()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "SELECT * FROM LogbookItems";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ToDoItem
                            {
                                Id = int.Parse(reader["ID"].ToString()),
                                Header = reader["Header"].ToString()
                            };

                            if (reader["CompleteDate"].ToString() != "")
                                item.Date = DateTime.MinValue.AddMilliseconds(long.Parse(reader["CompleteDate"].ToString()));

                            _toDoItemsCollection.Insert(0, item);
                        }
                    }
                }
            }
        }

        private void ToDoItem_OnUnChecked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}