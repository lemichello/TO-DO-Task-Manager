using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BUS.Models;
using BUS.Services;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for UpcomingPage.xaml
    /// </summary>
    public partial class UpcomingPage : Page
    {
        private readonly List<UpcomingToDoItems> _upcomingItemsCollection;
        private readonly ToDoItemService         _itemService;
        private readonly TagService              _tagService;
        private readonly MainWindow              _parent;
        private readonly int                     _userId;

        public UpcomingPage(MainWindow window, int userId)
        {
            InitializeComponent();

            _userId                  = userId;
            _upcomingItemsCollection = new List<UpcomingToDoItems>();
            _itemService             = new ToDoItemService(_userId);
            _tagService              = new TagService(_userId);
            _parent                  = window;

            FillCollection();

            UpcomingListView.ItemsSource = _upcomingItemsCollection;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var itemWindow = new ToDoItemWindow(_userId);

            itemWindow.ShowDialog();

            if (itemWindow.DialogResult == false) return;

            _itemService.Add(itemWindow.Item);

            if (itemWindow.Item.Id == -1)
                return;

            _tagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);

            _parent.UpdateUpcomingPage();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView) e.Source;

            if (listView.SelectedIndex == -1) return;

            var item = listView.SelectedItem as ToDoItemModel;
            var itemWindow = new ToDoItemWindow(item, _userId);

            listView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                _itemService.Remove(item);
                _parent.UpdateUpcomingPage();
                
                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            _itemService.Update(itemWindow.Item);
            _tagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);
            _parent.UpdateUpcomingPage();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = (ToDoItemModel) item;
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
            var toDoItem = (ToDoItemModel) item;

            toDoItem.Timer.Stop();
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            var timer    = (DispatcherTimer) sender;
            var toDoItem = (ToDoItemModel) timer.Tag;

            toDoItem.CompleteDay = DateTime.Today;

            _itemService.Update(toDoItem);
            
            toDoItem.Timer.Stop();

            _parent.UpdateUpcomingPage();
        }

        private void FillCollection()
        {
            // Fill next 7 days by TO-DO items.
            for (var i = 1; i <= 7; i++)
            {
                _upcomingItemsCollection.Add(new UpcomingToDoItems(ref i, _itemService, true));
            }

            for (var i = 0; i < 5; i++)
            {
                _upcomingItemsCollection.Add(new UpcomingToDoItems(ref i, _itemService, false));
            }
        }
    }
}