using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BUS.Models;
using BUS.Services;
using ToDoTaskManager.Classes;

namespace ToDoTaskManager
{
    /// <summary>
    /// Interaction logic for UpcomingPage.xaml
    /// </summary>
    public partial class UpcomingPage
    {
        private readonly List<UpcomingToDoItems> _upcomingItemsCollection;
        private readonly ToDoItemService         _itemService;
        private readonly TagService              _tagService;
        private readonly MainWindow              _parent;

        public UpcomingPage(MainWindow window)
        {
            InitializeComponent();

            _upcomingItemsCollection = new List<UpcomingToDoItems>();
            _itemService             = ToDoItemService.GetInstance();
            _tagService              = TagService.GetInstance();
            _parent                  = window;

            FillCollection();

            UpcomingListView.ItemsSource = _upcomingItemsCollection;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var itemWindow = new ToDoItemWindow(null);

            itemWindow.ShowDialog(DateTime.Today.AddDays(1));

            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.ProjectId = null;

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

            var item       = (ToDoItemModel) listView.SelectedItem;
            var itemWindow = new ToDoItemWindow(item.ProjectId, item);

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
                Interval = new TimeSpan(0, 0, 0, 1, 300)
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

            toDoItem.CompleteDay = DateTime.UtcNow;

            _itemService.Update(toDoItem);

            toDoItem.Timer.Stop();

            _parent.UpdateUpcomingPage();
        }

        private void FillCollection()
        {
            var allItems = _itemService.Get(i => i.CompleteDay == DateTime.MinValue.AddYears(1753)).ToList();

            // Fill next 7 days by TO-DO items.
            for (var i = 1; i <= 7; i++)
            {
                _upcomingItemsCollection.Add(new UpcomingToDoItems(ref i, allItems, true));
            }

            var iterationsCount = DateTime.Now.AddDays(8).Month != DateTime.Now.Month ? 6 : 5;

            for (var i = 0; i < iterationsCount; i++)
            {
                _upcomingItemsCollection.Add(new UpcomingToDoItems(ref i, allItems, false));
            }

            var yearsItems = allItems
                .Where(i => i.Date > DateTime.Today.AddMonths(5))
                .ToDictionary(item => item.Date.Year, item => allItems.Where(i => i.Date.Year == item.Date.Year));

            foreach (var i in yearsItems)
            {
                _upcomingItemsCollection.Add(new UpcomingToDoItems(i.Key, i.Value.ToList()));
            }
        }
    }
}