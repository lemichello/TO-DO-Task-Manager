using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using BUS.Models;
using BUS.Services;
using ToDoTaskManager.Classes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ToDoTaskManager
{
    public partial class SharedProjectPage : Page
    {
        private readonly ObservableCollection<ToDoItemView> _toDoItemsCollection;
        private readonly ToDoItemService                    _itemService;
        private readonly ProjectService                     _projectService;
        private readonly ToDoItemOperations                 _operations;
        private readonly int                                _projectId;
        private readonly MainWindow                         _parent;
        private          bool                               _membersLoaded;

        public SharedProjectPage(MainWindow parent, int projectId, string projectName, int userId,
            ToDoItemService itemService, ProjectService projectService, TagService tagService)
        {
            InitializeComponent();

            ProjectNameLabel.Content = projectName;

            _parent              = parent;
            _membersLoaded       = false;
            _projectId           = projectId;
            _toDoItemsCollection = new ObservableCollection<ToDoItemView>();
            _itemService         = itemService;
            _projectService      = projectService;
            _operations =
                new SharedToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, userId, _projectId,
                    itemService, tagService);

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
        }

        private void FillCollection()
        {
            var collection = _itemService.GetSharedProjectItems(_projectId).ToList();

            foreach (var i in collection)
            {
                _toDoItemsCollection.Add(_operations.ConvertToItemView(i));
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

        private void MembersExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (!_membersLoaded)
            {
                FillMembersExpander();
                _membersLoaded = true;
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

        private void InviteButton_OnCLick(object sender, RoutedEventArgs e)
        {
            InvitedUsersTextBox.Visibility = Visibility.Visible;
            ConfirmButton.Visibility       = Visibility.Visible;
            CancelButton.Visibility        = Visibility.Visible;

            InviteButton.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            InvitedUsersTextBox.Text       = "";
            InvitedUsersTextBox.Visibility = Visibility.Collapsed;

            ConfirmButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility  = Visibility.Collapsed;

            InviteButton.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            var userLogins = InvitedUsersTextBox.Text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (userLogins.Length == 0)
            {
                MessageBox.Show("You need to enter logins of user(s), which you want to invite");
                return;
            }

            // An error occured.
            if (_projectService.InviteUsers(new ProjectModel
            {
                Id   = _projectId,
                Name = (string) ProjectNameLabel.Content
            }, userLogins, false) == -1)
                return;

            MessageBox.Show("Invitation is successfully completed.");

            CancelButton_OnClick(null, null);
        }
    }
}