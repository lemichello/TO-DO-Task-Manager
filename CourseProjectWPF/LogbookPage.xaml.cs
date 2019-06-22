using ClassLibrary.Classes;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    public partial class LogbookPage
    {
        private readonly ObservableCollection<ToDoItem> _toDoItemsCollection;
        private readonly string                         _connectionString;
        private readonly MainWindow                     _parent;

        public LogbookPage(MainWindow window)
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItem>();
            _connectionString    = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            _parent              = window;

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ToDoItemsListView.SelectedIndex;

            if (index == -1) return;

            var itemWindow = new ToDoItemWindow(_toDoItemsCollection[index]);

            ToDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                DeleteToDoItem(_toDoItemsCollection[index]);
                _toDoItemsCollection.RemoveAt(index);

                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.CompleteDay = _toDoItemsCollection[index].CompleteDay;

            ReplaceToDoItem(itemWindow.Item);
            _parent.UpdateLogbookPage();
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
                            var item = CreateToDoItem(reader);

                            _toDoItemsCollection.Add(item);
                        }
                    }
                }
            }
        }

        private static ToDoItem CreateToDoItem(SQLiteDataReader reader)
        {
            var item = new ToDoItem
            {
                Id     = int.Parse(reader["ID"].ToString()),
                Header = reader["Header"].ToString(),
                Notes  = reader["Notes"].ToString()
            };

            if (reader["Date"].ToString() != "")
                item.Date = DateTime.MinValue.AddMilliseconds(long.Parse(reader["Date"].ToString()));

            if (reader["Deadline"].ToString() != "")
                item.Deadline =
                    DateTime.MinValue.AddMilliseconds(long.Parse(reader["Deadline"].ToString()));

            item.CompleteDay = reader["CompleteDay"].ToString();

            return item;
        }

        // Recover ToDoItem from Logbook page.
        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This TO-DO is already logged. " +
                                         "Are you sure you want to mark it as incomplete?",
                "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                ((CheckBox) sender).IsChecked = true;
                return;
            }

            var selectedItem = ((FrameworkElement) sender).DataContext;
            var index        = ToDoItemsListView.Items.IndexOf(selectedItem);
            var toDoItem     = _toDoItemsCollection[index];

            DeleteToDoItem(toDoItem);

            DatabaseOperations.AddToDoItem(toDoItem);
            _toDoItemsCollection.RemoveAt(index);
        }

        private void DeleteToDoItem(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string command = "DELETE FROM LogbookItems WHERE ID=@id";

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", item.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ReplaceToDoItem(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command =
                    "REPLACE INTO LogbookItems(ID, Header, Notes, Date, Deadline, CompleteDay) VALUES (@id, @header, @notes, @date, @deadline, @completeDay)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    FillCommandParameters(item, cmd);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void FillCommandParameters(ToDoItem item, SQLiteCommand cmd)
        {
            cmd.Parameters.AddWithValue("@id", item.Id);
            cmd.Parameters.AddWithValue("@header", item.Header);
            cmd.Parameters.AddWithValue("@notes", item.Notes);

            cmd.Parameters.AddWithValue("@date",
                item.Date != DateTime.MinValue
                    ? ((long) (item.Date - DateTime.MinValue).TotalMilliseconds).ToString()
                    : "");

            cmd.Parameters.AddWithValue("@deadline",
                item.Deadline != DateTime.MinValue ? item.Deadline.ToShortDateString() : "");

            cmd.Parameters.AddWithValue("@completeDay", item.CompleteDay);
        }
    }
}