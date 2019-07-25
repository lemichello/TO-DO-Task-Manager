using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BUS.Models;
using BUS.Services;
using CourseProjectWPF.Classes;

namespace CourseProjectWPF
{
    public partial class SharedProjectPage : Page
    {
        private readonly ObservableCollection<ToDoItemModel> _toDoItemsCollection;
        private readonly ToDoItemService                     _itemService;
        private readonly ToDoItemOperations                  _operations;
        private readonly int                                 _projectId;

        public SharedProjectPage(int projectId, string projectName, int userId)
        {
            InitializeComponent();

            ProjectNameLabel.Content = projectName;
            SharedLogoImage.Source   = new BitmapImage(new Uri(Path.GetFullPath("../../Resources/shared.png")));

            _projectId           = projectId;
            _toDoItemsCollection = new ObservableCollection<ToDoItemModel>();
            _itemService         = new ToDoItemService(userId);
            _operations =
                new SharedToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, userId, _projectId);

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void FillCollection()
        {
            var collection = _itemService.GetSharedProjectItems(_projectId).ToList();

            foreach (var i in collection)
            {
                _toDoItemsCollection.Add(i);
            }
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _operations.Selected();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _operations.Add();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            _operations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _operations.Unchecked(sender);
        }
    }
}