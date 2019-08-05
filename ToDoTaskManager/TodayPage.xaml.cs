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
    /// Interaction logic for TodayPage.xaml
    /// </summary>
    public partial class TodayPage
    {
        private readonly ObservableCollection<ToDoItemView> _toDoItemsCollection;
        private readonly TodayToDoItemOperations            _toDoItemOperations;
        private readonly ToDoItemService                    _service;

        public TodayPage()
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItemView>();
            _service             = ToDoItemService.GetInstance();

            _toDoItemOperations = new TodayToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, null);

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void FillCollection()
        {
            var minDate = DateTime.MinValue.AddYears(1753);

            var collection = _service.Get(item => (item.Date <= DateTime.Today && item.Date != minDate ||
                                                   item.Deadline <= DateTime.Today && item.Deadline != minDate) &&
                                                  item.CompleteDay == minDate).ToList();

            foreach (var i in collection)
            {
                _toDoItemsCollection.Add(_toDoItemOperations.ConvertToItemView(i));
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _toDoItemOperations.Selected();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Unchecked(sender);
        }
    }
}