using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BUS.Models;
using BUS.Services;

namespace CourseProjectWPF.Classes
{
    internal abstract class ToDoItemOperations
    {
        private readonly ListView                            _toDoItemsListView;
        private readonly ObservableCollection<ToDoItemModel> _toDoItemsCollection;
        private readonly ToDoItemService                     _itemService;
        private readonly TagService                          _tagService;
        private readonly int                                 _userId;

        internal ToDoItemOperations(ListView toDoItemsListView, ObservableCollection<ToDoItemModel> toDoItemsCollection,
            int userId)
        {
            _userId = userId;

            _toDoItemsListView   = toDoItemsListView;
            _toDoItemsCollection = toDoItemsCollection;
            _itemService         = new ToDoItemService(userId);
            _tagService          = new TagService(userId);
        }

        public void Add()
        {
            var itemWindow = new ToDoItemWindow(_userId);

            if (this is TodayToDoItemOperations)
                itemWindow.ShowDialog(DateTime.Today);
            else
                itemWindow.ShowDialog();

            if (itemWindow.DialogResult == false) return;

            _itemService.Add(itemWindow.Item);

            if (itemWindow.Item.Id != -1 && IsCorrect(itemWindow.Item))
                _toDoItemsCollection.Add(itemWindow.Item);
            else if (itemWindow.Item.Id == -1)
                return;

            _tagService.ReplaceItemsTags(itemWindow.Item.Id, itemWindow.SelectedTagsId);
        }

        public void Selected()
        {
            var index = _toDoItemsListView.SelectedIndex;

            if (index == -1) return;

            var item       = _toDoItemsCollection[index];
            var itemWindow = new ToDoItemWindow(item, _userId);

            _toDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                if (_itemService.Remove(item))
                    _toDoItemsCollection.Remove(item);

                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            _itemService.Update(itemWindow.Item);

            if (IsCorrect(itemWindow.Item))
                _toDoItemsCollection[index] = itemWindow.Item;
            else
                _toDoItemsCollection.Remove(item);

            _tagService.ReplaceItemsTags(item.Id, itemWindow.SelectedTagsId);
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            var timer    = (DispatcherTimer) sender;
            var toDoItem = (ToDoItemModel) timer.Tag;

            toDoItem.CompleteDay = DateTime.Now;

            _itemService.Update(toDoItem);

            _toDoItemsCollection.Remove(toDoItem);

            toDoItem.Timer.Stop();
        }

        public void Checked(object sender)
        {
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = _toDoItemsCollection[_toDoItemsListView.Items.IndexOf(item)];
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 1, 300)
            };

            timer.Tick += Timer_OnTick;
            timer.Tag  =  toDoItem;

            toDoItem.Timer = timer;

            timer.Start();
        }

        public void Unchecked(object sender)
        {
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = _toDoItemsCollection[_toDoItemsListView.Items.IndexOf(item)];

            toDoItem.Timer.Stop();
        }

        protected abstract bool IsCorrect(ToDoItemModel item);
    }

    internal sealed class InboxToDoItemOperations : ToDoItemOperations
    {
        public InboxToDoItemOperations(ListView toDoItemsListView,
            ObservableCollection<ToDoItemModel> toDoItemsCollection,
            int userId) :
            base(toDoItemsListView, toDoItemsCollection, userId)
        {
        }

        protected override bool IsCorrect(ToDoItemModel item)
        {
            // User didn't choose a date for task.
            return item.Date == DateTime.MinValue.AddYears(1753);
        }
    }

    internal sealed class TodayToDoItemOperations : ToDoItemOperations
    {
        public TodayToDoItemOperations(ListView toDoItemsListView,
            ObservableCollection<ToDoItemModel> toDoItemsCollection,
            int userId) :
            base(toDoItemsListView, toDoItemsCollection, userId)
        {
        }

        protected override bool IsCorrect(ToDoItemModel item)
        {
            // User chose today's date for task.
            return item.Date.ToShortDateString() == DateTime.Now.ToShortDateString();
        }
    }
}