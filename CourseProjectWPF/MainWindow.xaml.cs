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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _connectionString;

        public MainWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PagesListView.SelectedIndex == -1)
            {
                PagesFrame.Content = null;
                return;
            }

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

                default:
                    throw new NotImplementedException();
            }
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this);
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var enteredTags = SearchTextBox.Text.Split(new[] {' '},
                StringSplitOptions.RemoveEmptyEntries);

            if (enteredTags.Length == 0)
            {
                MessageBox.Show("You need to enter at least one tag to search");
                return;
            }

            FoundToDoItems.ItemsSource = null;

            SearchExpander.IsExpanded          = FillFoundListBox(enteredTags);
            SearchExpander.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private bool FillFoundListBox(string[] tags)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                var command = CreateSearchCommand(tags);

                connection.Open();

                using (var cmd = new SQLiteCommand(command, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return false;

                        var toDoItemsCollection = new ObservableCollection<ToDoItem>();

                        DatabaseOperations.FillCollection(reader, toDoItemsCollection);

                        FoundToDoItems.ItemsSource = toDoItemsCollection;
                    }
                }
            }

            return true;
        }

        private static string CreateSearchCommand(string[] tags)
        {
            var command =
                "SELECT a.ID, a.Header, a.Notes, a.Date, a.Deadline FROM ItemsTags it1 INNER JOIN ToDoItems a ON a.ID=it1.ItemID";

            for (var i = 0; i < tags.Length; i++)
            {
                command += $" INNER JOIN ItemsTags it{i + 2} ON it1.ItemID=it{i + 2}.ItemID";
            }

            command += $" WHERE it1.TagID=(SELECT ID FROM Tags WHERE Text='{tags[0]}')";

            for (var i = 1; i < tags.Length; i++)
            {
                command += $" AND it{i + 1}.TagID=(SELECT ID FROM Tags WHERE Text='{tags[i]}')";
            }

            command += " GROUP BY a.ID";

            return command;
        }

        private void FoundToDoItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = FoundToDoItems.SelectedIndex;

            if (index == -1) return;

            var selectedToDoItem = (ToDoItem) FoundToDoItems.Items[index];

            if (selectedToDoItem.Date == DateTime.MinValue)
                PagesListView.SelectedIndex = 0;

            else if (selectedToDoItem.Date == DateTime.Today)
                PagesListView.SelectedIndex = 1;

            else
                PagesListView.SelectedIndex = 2;
        }
    }
}