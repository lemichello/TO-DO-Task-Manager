using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BUS.Models;
using BUS.Services;

namespace ToDoTaskManager
{
    public partial class LogbookPage
    {
        private readonly ObservableCollection<LogbookToDoItem> _toDoItemsCollection;
        private readonly ToDoItemService                       _itemService;
        private readonly TagService                            _tagService;
        private readonly MainWindow                            _parent;
        private readonly int                                   _userId;

        public LogbookPage(MainWindow window, int userId, ToDoItemService itemService, TagService tagService)
        {
            InitializeComponent();

            _userId              = userId;
            _toDoItemsCollection = new ObservableCollection<LogbookToDoItem>();
            _itemService         = itemService;
            _tagService          = tagService;
            _parent              = window;

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private class LogbookToDoItem : ToDoItemModel
        {
            public string CompleteDateStr { get; set; }
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ToDoItemsListView.SelectedIndex;

            if (index == -1) return;

            var item       = _toDoItemsCollection[index];
            var itemWindow = new ToDoItemWindow(_toDoItemsCollection[index], _userId);

            ToDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                _itemService.Remove(item);
                _parent.UpdateLogbookPage();

                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.CompleteDay = item.CompleteDay;

            _itemService.Update(itemWindow.Item);
            _tagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);
            _parent.UpdateLogbookPage();
        }

        private void FillCollection()
        {
            var items = _itemService.Get(i => i.CompleteDay != DateTime.MinValue.AddYears(1753)).ToList();

            items.Sort((a, b) => -a.CompleteDay.CompareTo(b.CompleteDay));

            foreach (var i in items)
            {
                _toDoItemsCollection.Add(new LogbookToDoItem
                {
                    Id              = i.Id,
                    Header          = i.Header,
                    Notes           = i.Notes,
                    Date            = i.Date,
                    Deadline        = i.Deadline,
                    CompleteDay     = i.CompleteDay,
                    CompleteDateStr = i.CompleteDay.ToShortDateString(),
                    ProjectName     = i.ProjectName,
                    ProjectId       = i.ProjectId
                });
            }
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

            toDoItem.CompleteDay = DateTime.MinValue.AddYears(1753);

            _itemService.Update(toDoItem);
            _toDoItemsCollection.RemoveAt(index);
        }
    }
}