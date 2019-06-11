using ClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for UpcomingPage.xaml
    /// </summary>
    public partial class UpcomingPage : Page
    {
        private readonly string                  _connectionString;
        private readonly List<UpcomingToDoItems> _upcomingItemsCollection;
        private readonly MainWindow              _parent;

        public UpcomingPage(MainWindow window)
        {
            InitializeComponent();

            _upcomingItemsCollection = new List<UpcomingToDoItems>();
            _connectionString        = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            _parent                  = window;

            FillCollection();

            UpcomingListView.ItemsSource = _upcomingItemsCollection;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var itemWindow = new ToDoItemWindow();

            itemWindow.ShowDialog();

            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.Id = MainWindow.AddToDoItem(itemWindow.Item);

            MainWindow.AddTagsToItem(itemWindow.Item.Id, itemWindow.SelectedTags);

            _parent.UpdateUpcomingPage();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView) e.Source;

            if (listView.SelectedIndex == -1) return;

            var item       = listView.SelectedItem as ToDoItem;
            var itemWindow = new ToDoItemWindow(item);

            listView.SelectedIndex = -1;
            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                MainWindow.RemoveToDoItem(item);
                _parent.UpdateUpcomingPage();

                return;
            }

            if (itemWindow.DialogResult == false) return;

            MainWindow.ReplaceToDoItem(itemWindow.Item);
            MainWindow.ReplaceToDoItemTags(itemWindow.Item.Id, itemWindow.SelectedTags);

            _parent.UpdateUpcomingPage();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).DataContext;

            MainWindow.RemoveTagsFromToDoItem((item as ToDoItem).Id);
            MainWindow.RemoveToDoItem(item as ToDoItem);
            MainWindow.AddToLogbook(item as ToDoItem);

            _parent.UpdateUpcomingPage();
        }

        private void FillCollection()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Fill next 7 days by TO-DO items.
                for (var i = 1; i <= 7; i++)
                {
                    _upcomingItemsCollection.Add(new UpcomingToDoItems(ref i, connection, true));
                }

                for (var i = 0; i < 5; i++)
                {
                    _upcomingItemsCollection.Add(new UpcomingToDoItems(ref i, connection, false));
                }
            }
        }
    }
}