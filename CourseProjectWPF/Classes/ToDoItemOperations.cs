using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClassLibrary.Classes;

namespace CourseProjectWPF.Classes
{
    internal abstract class ToDoItemOperations
    {
        private readonly ListView                       _toDoItemsListView;
        private readonly ObservableCollection<ToDoItem> _toDoItemsCollection;

        internal ToDoItemOperations(ListView toDoItemsListView, ObservableCollection<ToDoItem> toDoItemsCollection)
        {
            _toDoItemsListView   = toDoItemsListView;
            _toDoItemsCollection = toDoItemsCollection;
        }

        public void Selected()
        {
            var index = _toDoItemsListView.SelectedIndex;

            if (index == -1) return;

            var itemWindow = new ToDoItemWindow(_toDoItemsCollection[index]);

            _toDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                DatabaseOperations.RemoveToDoItem(_toDoItemsCollection[index]);
                _toDoItemsCollection.RemoveAt(index);

                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            DatabaseOperations.ReplaceToDoItem(itemWindow.Item);
            DatabaseOperations.ReplaceToDoItemTags(itemWindow.Item.Id, itemWindow.SelectedTags);

            if (IsCorrect(itemWindow.Item))
                _toDoItemsCollection[index] = itemWindow.Item;
            else
                _toDoItemsCollection.RemoveAt(index);
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer) sender;
            var toDoItem = (ToDoItem) timer.Tag;

            DatabaseOperations.RemoveTagsFromToDoItem(toDoItem.Id);
            DatabaseOperations.RemoveToDoItem(toDoItem);
            DatabaseOperations.AddToDoItemToLogbook(toDoItem);
            
            _toDoItemsCollection.Remove(toDoItem);
            
            toDoItem.Timer.Stop();
        }
        
        public void Checked(object sender)
        {
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = _toDoItemsCollection[_toDoItemsListView.Items.IndexOf(item)];
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 2)
            };
            
            timer.Tick += Timer_OnTick;
            timer.Tag = toDoItem;

            toDoItem.Timer = timer;
            
            timer.Start();
        }

        public void Unchecked(object sender)
        {
            var item     = ((FrameworkElement) sender).DataContext;
            var toDoItem = _toDoItemsCollection[_toDoItemsListView.Items.IndexOf(item)];
            
            toDoItem.Timer.Stop();
        }
        
        public void Add()
        {
            var itemWindow = new ToDoItemWindow();

            // Set today's initial date if user on "Today" page.
            if (this is TodayToDoItemOperations)
                itemWindow.ShowDialog(DateTime.Now);
            else
                itemWindow.ShowDialog();

            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.Id = DatabaseOperations.AddToDoItem(itemWindow.Item);

            if (IsCorrect(itemWindow.Item))
                _toDoItemsCollection.Add(itemWindow.Item);

            DatabaseOperations.AddTagsToItem(itemWindow.Item.Id, itemWindow.SelectedTags);
        }

        protected abstract bool IsCorrect(ToDoItem item);
    }

    internal sealed class InboxToDoItemOperations : ToDoItemOperations
    {
        public InboxToDoItemOperations(ListView toDoItemsListView, ObservableCollection<ToDoItem> toDoItemsCollection) :
            base(toDoItemsListView, toDoItemsCollection)
        {
        }

        protected override bool IsCorrect(ToDoItem item)
        {
            // User didn't choose a date for task.
            return item.Date == DateTime.MinValue;
        }
    }

    internal sealed class TodayToDoItemOperations : ToDoItemOperations
    {
        public TodayToDoItemOperations(ListView toDoItemsListView, ObservableCollection<ToDoItem> toDoItemsCollection) :
            base(toDoItemsListView, toDoItemsCollection)
        {
        }

        protected override bool IsCorrect(ToDoItem item)
        {
            // User chose today's date for task.
            return item.Date.ToShortDateString() == DateTime.Now.ToShortDateString();
        }
    }
}