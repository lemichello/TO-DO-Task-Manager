using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using ClassLibrary.Classes;

namespace CourseProjectWPF.Classes
{
    public static class DatabaseOperations
    {
        private static readonly string ConnectionString;

        static DatabaseOperations()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        }
        
        public static long AddToDoItem(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                const string command =
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

                return GetLastWrittenId(connection);
            }
        }

        public static void RemoveToDoItem(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
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
            using (var connection = new SQLiteConnection(ConnectionString))
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

        public static void AddToDoItemToLogbook(ToDoItem item)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
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

        public static void AddTagsToItem(long itemId, IEnumerable<Tag> tags)
        {
            if (tags == null) return;

            using (var connection = new SQLiteConnection(ConnectionString))
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
            using (var connection = new SQLiteConnection(ConnectionString))
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
            using (var connection = new SQLiteConnection(ConnectionString))
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

        private static string GetMilliseconds(DateTime item)
        {
            return ((long) (item.Date - DateTime.MinValue).TotalMilliseconds).ToString();
        }

        public static long GetLastWrittenId(SQLiteConnection connection)
        {
            using (var cmd = new SQLiteCommand("SELECT last_insert_rowid()", connection))
            {
                return (long) cmd.ExecuteScalar();
            }
        }
    }
}