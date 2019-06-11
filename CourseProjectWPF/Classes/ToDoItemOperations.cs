using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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

            if (index == -1)
                return;

            var itemWindow = new ToDoItemWindow(_toDoItemsCollection[index]);

            _toDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                MainWindow.RemoveToDoItem(_toDoItemsCollection[index]);
                _toDoItemsCollection.RemoveAt(index);

                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            MainWindow.ReplaceToDoItem(itemWindow.Item);

            if (IsCorrect(itemWindow.Item))
                _toDoItemsCollection[index] = itemWindow.Item;
            else
                _toDoItemsCollection.RemoveAt(index);
        }

        public void Checked(object sender)
        {
            var item = ((FrameworkElement) sender).DataContext;

            MainWindow.RemoveToDoItem(_toDoItemsCollection[_toDoItemsListView.Items.IndexOf(item)]);
            MainWindow.AddToLogbook(_toDoItemsCollection[_toDoItemsListView.Items.IndexOf(item)]);

            _toDoItemsCollection.RemoveAt(_toDoItemsListView.Items.IndexOf(item));
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

            itemWindow.Item.Id = MainWindow.AddToDoItem(itemWindow.Item);

            if (IsCorrect(itemWindow.Item))
                _toDoItemsCollection.Add(itemWindow.Item);
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