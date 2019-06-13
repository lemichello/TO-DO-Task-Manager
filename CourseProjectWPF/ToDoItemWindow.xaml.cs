using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClassLibrary.Classes;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    public partial class ToDoItemWindow : Window
    {
        private readonly ObservableCollection<Tag> _tagsList;
        private readonly string                    _connectionString;
        
        public ToDoItem Item { get; }
        public bool ToDelete { get; private set; }
        public List<Tag> SelectedTags { get; private set; }

        public ToDoItemWindow()
        {
            InitializeComponent();

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
            Item                            = new ToDoItem();
            ToDelete                        = false;

            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            _tagsList         = new ObservableCollection<Tag>();

            FillTagsCollection();

            TagsListBox.ItemsSource = _tagsList;
        }

        public void ShowDialog(DateTime dateTime)
        {
            PickedDate.SelectedDate = dateTime;

            ShowDialog();
        }

        public ToDoItemWindow(ToDoItem item) : this()
        {
            Item = new ToDoItem {Id = item.Id};

            HeaderText.Text = item.Header;
            NotesText.Text  = item.Notes;

            if (item.Date != DateTime.MinValue)
                PickedDate.SelectedDate = item.Date;

            if (item.Deadline != DateTime.MinValue)
                PickedDeadline.SelectedDate = item.Deadline;

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;

            SelectTags();
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

            if (TagsListBox.SelectedItems.Count != 0)
                FillSelectedList();

            DialogResult = true;
            Close();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            ToDelete = true;
            DatabaseOperations.RemoveTagsFromToDoItem(Item.Id);

            Close();
        }

        private void FillTagsCollection()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "SELECT * FROM Tags";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _tagsList.Add(new Tag
                            {
                                Id   = long.Parse(reader["ID"].ToString()),
                                Text = reader["Text"].ToString()
                            });
                        }
                    }
                }
            }
        }

        private void FillSelectedList()
        {
            SelectedTags = new List<Tag>();

            foreach (Tag i in TagsListBox.SelectedItems)
            {
                SelectedTags.Add(i);
            }
        }

        private void SelectTags()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                // Selecting all tags, which has TO-DO item.
                const string command =
                    "SELECT a.ID, a.Text FROM Tags AS a, ItemsTags AS b WHERE @id=b.ItemID AND a.ID=b.TagID";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", Item.Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tag = new Tag
                            {
                                Id   = long.Parse(reader["ID"].ToString()),
                                Text = reader["Text"].ToString()
                            };

                            TagsListBox.SelectedItems.Add(tag);
                        }
                    }
                }
            }
        }

        private void AddButton_OnCLick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            var tag = new Tag {Text = window.NewText, Id = AddTag(window.NewText)};

            _tagsList.Add(tag);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TagsListBox.SelectedItems.Count == 0) return;

            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            _tagsList[TagsListBox.SelectedIndex].Text = window.NewText;
            ReplaceTag(_tagsList[TagsListBox.SelectedIndex]);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TagsListBox.SelectedItems.Count == 0) return;

            var id = _tagsList[TagsListBox.SelectedIndex].Id;

            _tagsList.RemoveAt(TagsListBox.SelectedIndex);
            RemoveTagFromDatabase(id);
        }

        private long AddTag(string text)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "INSERT INTO Tags(Text) VALUES (@text)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@text", text);

                    cmd.ExecuteNonQuery();
                }

                return DatabaseOperations.GetLastWrittenId(connection);
            }
        }

        private void ReplaceTag(Tag tag)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "REPLACE INTO Tags(ID, Text) VALUES (@id, @text)";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", tag.Id);
                    cmd.Parameters.AddWithValue("@text", tag.Text);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void RemoveTagFromDatabase(long id)
        {
            DatabaseOperations.RemoveTagConnections(id);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string command = "DELETE FROM Tags WHERE ID=@id";

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    cmd.Prepare();

                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}