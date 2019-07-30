using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BUS.Models;
using BUS.Services;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for TodayPage.xaml
    /// </summary>
    public partial class TodayPage : Page
    {
        private readonly ObservableCollection<ToDoItemView> _toDoItemsCollection;
        private readonly TodayToDoItemOperations            _toDoItemOperations;
        private readonly ToDoItemService                    _service;

        public TodayPage(int userId, ToDoItemService itemService, TagService tagService)
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItemView>();
            _service             = itemService;

            _toDoItemOperations = new TodayToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, userId, null,
                itemService, tagService);

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void FillCollection()
        {
            var collection = _service.Get(item => (item.Date <= DateTime.Today ||
                                                   item.Deadline == DateTime.Today) &&
                                                  item.CompleteDay == DateTime.MinValue.AddYears(1753) &&
                                                  (item.Date != DateTime.MinValue.AddYears(1753) ||
                                                   item.Deadline == DateTime.Today)).ToList();

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