using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BUS.Services;
using ToDoTaskManager.Classes;

namespace ToDoTaskManager
{
    /// <summary>
    /// Interaction logic for InboxPage.xaml
    /// </summary>
    public partial class InboxPage
    {
        private readonly ObservableCollection<ToDoItemView> _toDoItemsCollection;
        private readonly ToDoItemService                    _itemService;
        private readonly ToDoItemOperations                 _operations;

        public InboxPage()
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItemView>();
            _itemService         = ToDoItemService.GetInstance();
            _operations          = new InboxToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, null);

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            _operations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _operations.Unchecked(sender);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _operations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _operations.Selected();
        }

        private void FillCollection()
        {
            var minDate = DateTime.MinValue.AddYears(1753);
            
            var collection = _itemService.Get(item =>
                item.Date == minDate &&
                item.CompleteDay == minDate &&
                item.ProjectName == "").ToList();

            foreach (var i in collection)
            {
                var item = _operations.ConvertToItemView(i);

                if (item == null) continue;

                _toDoItemsCollection.Add(item);
            }
        }
    }
}