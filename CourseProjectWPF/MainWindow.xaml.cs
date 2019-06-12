using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ClassLibrary.Classes;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static string _connectionString;

        public MainWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (PagesListView.SelectedIndex)
            {
                case 0:
                    PagesFrame.Content = new InboxPage();
                    break;

                case 1:
                    PagesFrame.Content = new TodayPage();
                    break;

                case 2:
                    PagesFrame.Content = new UpcomingPage(this);
                    break;

                case 3:
                    PagesFrame.Content = new LogbookPage(this);
                    break;
            }
        }

        public static long AddToDoItem(ToDoItem item)
        {
            long id;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                var command =
                    "INSERT INTO ToDoItems(Header, Notes, Date, Deadline) VALUES (@header, @notes, @date, @deadline)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@header", item.Header);
                    cmd.Parameters.AddWithValue("@notes", item.Notes);

                    cmd.Parameters.AddWithValue("@date",
                        item.Date != DateTime.MinValue
                            ? GetMilliseconds(item.Date)
                            : "");

                    cmd.Parameters.AddWithValue("@deadline",
                        item.Deadline != DateTime.MinValue ? item.Deadline.ToShortDateString() : "");

                    cmd.ExecuteNonQuery();
                }

                command = "SELECT last_insert_rowid()";

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    id = (long) cmd.ExecuteScalar();
                }
            }

            return id;
        }

        public static void RemoveToDoItem(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "DELETE FROM ToDoItems WHERE ID=@id";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", item.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ReplaceToDoItem(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command =
                    "REPLACE INTO ToDoItems(ID, Header, Notes, Date, Deadline) VALUES (@id, @header, @notes, @date, @deadline)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", item.Id);
                    cmd.Parameters.AddWithValue("@header", item.Header);
                    cmd.Parameters.AddWithValue("@notes", item.Notes);

                    cmd.Parameters.AddWithValue("@date",
                        item.Date != DateTime.MinValue
                            ? GetMilliseconds(item.Date)
                            : "");

                    cmd.Parameters.AddWithValue("@deadline",
                        item.Deadline != DateTime.MinValue ? item.Deadline.ToShortDateString() : "");

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void FillCollection(SQLiteDataReader reader, ObservableCollection<ToDoItem> toDoItemsCollection)
        {
            while (reader.Read())
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
                    item.Deadline = DateTime.Parse(reader["Deadline"].ToString());

                toDoItemsCollection.Add(item);
            }
        }

        public static void AddToLogbook(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command =
                    "INSERT INTO LogbookItems(ID, Header, Notes, Date, Deadline, CompleteDay) VALUES(@id, @header, @notes, @date, @deadline, @completeDay)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", item.Id);
                    cmd.Parameters.AddWithValue("@header", item.Header);
                    cmd.Parameters.AddWithValue("@notes", item.Notes);
                    cmd.Parameters.AddWithValue("@date", GetMilliseconds(item.Date));
                    cmd.Parameters.AddWithValue("@deadline", GetMilliseconds(item.Deadline));
                    cmd.Parameters.AddWithValue("@completeDay", DateTime.Today.ToShortDateString());

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string GetMilliseconds(DateTime item)
        {
            return ((long) (item.Date - DateTime.MinValue).TotalMilliseconds).ToString();
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this);
        }

        public static void AddTagsToItem(long itemId, IEnumerable<Tag> tags)
        {
            if (tags == null) return;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "INSERT INTO ItemsTags(ItemID, TagID) VALUES (@itemId, @tagId)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    foreach (var i in tags)
                    {
                        cmd.Prepare();

                        cmd.Parameters.AddWithValue("@itemId", itemId);
                        cmd.Parameters.AddWithValue("@tagId", i.Id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void ReplaceToDoItemTags(long itemId, IEnumerable<Tag> tags)
        {
            RemoveTagsFromToDoItem(itemId);

            AddTagsToItem(itemId, tags);
        }

        public static void RemoveTagsFromToDoItem(long itemId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "DELETE FROM ItemsTags WHERE ItemID=@id";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", itemId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Removes all connections of tag from ItemsTags.
        public static void RemoveTagConnections(long id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "DELETE FROM ItemsTags WHERE TagID=@id";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void Search_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var enteredTags = SearchTextBox.Text.Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            if (enteredTags.Length == 0)
            {
                MessageBox.Show("You need to enter at least one tag to search");
                return;
            }

            FoundToDoItems.Items.Clear();
        }

        private bool FillFoundListBox(string[] tags)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "SELECT * FROM Tags WHERE Text LIKE %@tagText%";
                string searchCommand = "SELECT * FROM ToDoItems WHERE ";

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    foreach (var i in tags)
                    {
                        cmd.Prepare();

                        cmd.Parameters.AddWithValue("@tagText", i);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}