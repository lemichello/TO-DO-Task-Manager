using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BUS.Models;
using BUS.Services;

namespace ToDoTaskManager.Classes
{
    internal abstract class ToDoItemOperations
    {
        private readonly   ListView                           _toDoItemsListView;
        private readonly   ObservableCollection<ToDoItemView> _toDoItemsCollection;
        protected readonly ToDoItemService                    ItemService;
        protected readonly DateTime                           MinDate;
        private readonly   TagService                         _tagService;
        private readonly   int?                               _projectId;

        internal ToDoItemOperations(ListView toDoItemsListView, ObservableCollection<ToDoItemView> toDoItemsCollection,
            int? projectId)
        {
            _projectId = projectId;

            _toDoItemsListView   = toDoItemsListView;
            _toDoItemsCollection = toDoItemsCollection;
            ItemService          = ToDoItemService.GetInstance();
            _tagService          = TagService.GetInstance();
            MinDate             = DateTime.MinValue.AddYears(1753);
        }

        public void Add()
        {
            var itemWindow = new ToDoItemWindow(_projectId);

            if (this is TodayToDoItemOperations)
                itemWindow.ShowDialog(DateTime.Today);
            else
                itemWindow.ShowDialog();

            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.ProjectId = _projectId;

            ItemService.Add(itemWindow.Item);

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
            var itemWindow = new ToDoItemWindow(_projectId, item);

            _toDoItemsListView.SelectedItem = null;

            itemWindow.ShowDialog();

            if (itemWindow.ToDelete)
            {
                if (ItemService.Remove(item))
                    _toDoItemsCollection.Remove(item);

                return;
            }

            // User closed a window.
            if (itemWindow.DialogResult == false) return;

            itemWindow.Item.ProjectName = item.ProjectName;

            ItemService.Update(itemWindow.Item);

            if (IsCorrect(itemWindow.Item))
                _toDoItemsCollection[index] = itemWindow.Item;
            else
                _toDoItemsCollection.Remove(item);

            _tagService.ReplaceItemsTags(item.Id, itemWindow.SelectedTagsId);
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            var timer    = (DispatcherTimer) sender;
            var toDoItem = (ToDoItemView) timer.Tag;

            toDoItem.CompleteDay = DateTime.Now;

            ItemService.Update(toDoItem);

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

        protected static ToDoItemView GetItemView(ToDoItemModel item)
        {
            return new ToDoItemView
            {
                Id          = item.Id,
                Header      = item.Header,
                Notes       = item.Notes,
                Date        = item.Date,
                Deadline    = item.Deadline,
                CompleteDay = item.CompleteDay,
                ProjectId   = item.ProjectId,
                ProjectName = item.ProjectName,
                Timer       = item.Timer
            };
        }

        public abstract ToDoItemView ConvertToItemView(ToDoItemModel item);

        protected abstract bool IsCorrect(ToDoItemView item);
    }

    internal sealed class InboxToDoItemOperations : ToDoItemOperations
    {
        public InboxToDoItemOperations(ListView toDoItemsListView,
            ObservableCollection<ToDoItemView> toDoItemsCollection, int? projectId) :
            base(toDoItemsListView, toDoItemsCollection, projectId)
        {
        }

        protected override bool IsCorrect(ToDoItemView item)
        {
            // User didn't choose a date for task.
            return item.Date == MinDate;
        }

        public override ToDoItemView ConvertToItemView(ToDoItemModel item)
        {
            var itemView = GetItemView(item);

            if (itemView.Deadline == MinDate)
                return itemView;

            // Moving task, which deadline is today, to TodayPage.
            if (itemView.Deadline <= DateTime.Today)
            {
                itemView.Date = DateTime.Today;
                ItemService.Update(itemView);

                return null;
            }

            var remainingDays = (itemView.Deadline - DateTime.Today).TotalDays;

            itemView.DeadlineColor = "Gray";
            itemView.DeadlineShort = $"{remainingDays}d left";

            return itemView;
        }
    }

    internal sealed class TodayToDoItemOperations : ToDoItemOperations
    {
        public TodayToDoItemOperations(ListView toDoItemsListView,
            ObservableCollection<ToDoItemView> toDoItemsCollection, int? projectId) :
            base(toDoItemsListView, toDoItemsCollection, projectId)
        {
        }

        protected override bool IsCorrect(ToDoItemView item)
        {
            // User chose today's date for task.
            return item.Date == DateTime.Now.Date ||
                   item.Deadline <= DateTime.Today && item.Deadline != MinDate;
        }

        public override ToDoItemView ConvertToItemView(ToDoItemModel item)
        {
            var itemView = GetItemView(item);

            if (itemView.Deadline == MinDate)
                return itemView;

            if (itemView.Deadline <= DateTime.Today)
            {
                itemView.DeadlineColor = "Red";
                itemView.DeadlineShort = "today";
            }
            else
            {
                var remainingDays = (itemView.Deadline - DateTime.Today).TotalDays;

                itemView.DeadlineColor = "Gray";
                itemView.DeadlineShort = $"{remainingDays}d left";
            }

            return itemView;
        }
    }

    internal sealed class SharedToDoItemOperations : ToDoItemOperations
    {
        public SharedToDoItemOperations(ListView toDoItemsListView,
            ObservableCollection<ToDoItemView> toDoItemsCollection, int? projectId) : base(toDoItemsListView,
            toDoItemsCollection, projectId)
        {
        }

        protected override bool IsCorrect(ToDoItemView item)
        {
            return true;
        }

        public override ToDoItemView ConvertToItemView(ToDoItemModel item)
        {
            var itemView = GetItemView(item);

            if (itemView.Deadline == MinDate)
                return itemView;

            if (itemView.Deadline <= DateTime.Today)
            {
                itemView.DeadlineColor = "Red";
                itemView.DeadlineShort = "today";
            }
            else
            {
                var remainingDays = (itemView.Deadline - DateTime.Today).TotalDays;

                itemView.DeadlineColor = "Gray";
                itemView.DeadlineShort = $"{remainingDays}d left";
            }

            return itemView;
        }
    }
}