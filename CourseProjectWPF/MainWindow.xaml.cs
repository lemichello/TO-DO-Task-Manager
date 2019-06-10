using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows.Controls;
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
                    PagesFrame.Content = new LogbookPage();
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
                    Header = reader["Header"].ToString(), Notes = reader["Notes"].ToString()
                };

                if (reader["Date"].ToString() != "")
                    item.Date = DateTime.MinValue.AddMilliseconds(long.Parse(reader["Date"].ToString()));

                if (reader["Deadline"].ToString() != "")
                    item.Deadline = DateTime.Parse(reader["Deadline"].ToString());

                toDoItemsCollection.Add(item);
            }
        }

        private static string GetMilliseconds(DateTime time)
        {
            return ((long) (time.Date - DateTime.MinValue).TotalMilliseconds).ToString();
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this);
        }
    }
}