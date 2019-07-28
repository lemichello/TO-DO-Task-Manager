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
    /// Interaction logic for InboxPage.xaml
    /// </summary>
    public partial class InboxPage : Page
    {
        private readonly ObservableCollection<ToDoItemModel> _toDoItemsCollection;
        private readonly ToDoItemService                     _service;
        private readonly ToDoItemOperations                  _operations;

        public InboxPage(int userId, ToDoItemService service, TagService tagService)
        {
            InitializeComponent();

            _toDoItemsCollection = new ObservableCollection<ToDoItemModel>();
            _service             = service;
            _operations          = new InboxToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, userId, null,
                service, tagService);

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
            var collection = _service.Get(item =>
                item.Date == DateTime.MinValue.AddYears(1753) &&
                item.CompleteDay == DateTime.MinValue.AddYears(1753) &&
                item.ProjectName == "").ToList();

            foreach (var i in collection)
            {
                _toDoItemsCollection.Add(i);
            }
        }
    }
}