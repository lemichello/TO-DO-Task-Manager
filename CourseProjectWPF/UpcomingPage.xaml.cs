using ClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
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

            itemWindow.Item.Id = DatabaseOperations.AddToDoItem(itemWindow.Item);

            DatabaseOperations.AddTagsToItem(itemWindow.Item.Id, itemWindow.SelectedTags);

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
                DatabaseOperations.RemoveToDoItem(item);
                _parent.UpdateUpcomingPage();

                return;
            }

            if (itemWindow.DialogResult == false) return;

            DatabaseOperations.ReplaceToDoItem(itemWindow.Item);
            DatabaseOperations.ReplaceToDoItemTags(itemWindow.Item.Id, itemWindow.SelectedTags);

            _parent.UpdateUpcomingPage();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            /*var item = ((FrameworkElement) sender).DataContext;

            DatabaseOperations.RemoveTagsFromToDoItem(((ToDoItem) item).Id);
            DatabaseOperations.RemoveToDoItem(item as ToDoItem);
            DatabaseOperations.AddToDoItemToLogbook(item as ToDoItem);

            _parent.UpdateUpcomingPage();*/
            
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = (ToDoItem) item;
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 2)
            };
            
            timer.Tick += Timer_OnTick;
            timer.Tag  =  toDoItem;

            toDoItem.Timer = timer;
            
            timer.Start();
        }
        
        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = (ToDoItem) item;
            
            toDoItem.Timer.Stop();
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            var timer    = (DispatcherTimer) sender;
            var toDoItem = (ToDoItem) timer.Tag;
            
            DatabaseOperations.RemoveTagsFromToDoItem(toDoItem.Id);
            DatabaseOperations.RemoveToDoItem(toDoItem);
            DatabaseOperations.AddToDoItemToLogbook(toDoItem);

            _parent.UpdateUpcomingPage();
            
            toDoItem.Timer.Stop();
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