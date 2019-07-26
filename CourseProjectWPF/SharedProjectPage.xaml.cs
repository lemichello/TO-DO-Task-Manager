using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using BUS.Models;
using BUS.Services;
using CourseProjectWPF.Classes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CourseProjectWPF
{
    public partial class SharedProjectPage : Page
    {
        private readonly ObservableCollection<ToDoItemModel> _toDoItemsCollection;
        private readonly ToDoItemService                     _itemService;
        private readonly ProjectService                      _projectService;
        private readonly ToDoItemOperations                  _operations;
        private readonly int                                 _projectId;
        private readonly MainWindow                          _parent;

        public SharedProjectPage(MainWindow parent, int projectId, string projectName, int userId,
            ToDoItemService itemService,
            ProjectService projectService)
        {
            InitializeComponent();
            
            ProjectNameLabel.Content = projectName;
            SharedLogoImage.Source   = new BitmapImage(new Uri(Path.GetFullPath("../../Resources/shared.png")));

            _parent              = parent;
            _projectId           = projectId;
            _toDoItemsCollection = new ObservableCollection<ToDoItemModel>();
            _itemService         = itemService;
            _projectService      = projectService;
            _operations =
                new SharedToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, userId, _projectId);

            FillCollection();
            FillMembersExpander();

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

        private void FillMembersExpander()
        {
            var members = _projectService.GetProjectMembers(_projectId).ToList();

            foreach (var i in members)
            {
                MembersExpander.Content += i + "\n";
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

        private void LeaveProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure? This will delete all TO-DO items from this project.",
                "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.No)
                return;
            
            _projectService.LeaveProject(_projectId);
            _parent.RemoveProject();
        }
    }
}