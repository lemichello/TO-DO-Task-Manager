using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly LoginWindow                                  _parent;
        private readonly int                                          _userId;
        private readonly TagService                                   _tagService;
        private readonly ToDoItemService                              _itemService;
        private readonly ProjectService                               _projectService;
        private readonly ObservableCollection<ProjectView>            _projects;
        private readonly ObservableCollection<InvitationRequestModel> _invitations;
        private          bool                                         _isLogOut;

        public MainWindow(LoginWindow parent, int userId)
        {
            InitializeComponent();

            _projectService = new ProjectService(userId);
            _itemService    = new ToDoItemService(userId);
            _invitations    = new ObservableCollection<InvitationRequestModel>();
            _projects       = new ObservableCollection<ProjectView>();

            ShowDefaultProjects();
            ShowSharedProjects();
            ShowInvitations();

            ProjectsListView.ItemsSource    = _projects;
            InvitationsListView.ItemsSource = _invitations;

            RefreshButtonImage.Source = new BitmapImage(new Uri(Path.GetFullPath("../../Resources/refresh.png")));

            _isLogOut   = false;
            _parent     = parent;
            _userId     = userId;
            _tagService = new TagService(_userId);
        }

        private void ShowDefaultProjects()
        {
            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/inbox.png",
                Name        = "Inbox"
            });

            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/today.png",
                Name        = "Today"
            });

            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/upcoming.png",
                Name        = "Upcoming"
            });

            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/logbook.png",
                Name        = "Logbook"
            });
        }

        private void ShowSharedProjects()
        {
            var foundProjects = _projectService.GetProjects().ToList();

            foreach (var project in foundProjects)
            {
                _projects.Add(new ProjectView
                {
                    Id          = project.Id,
                    ImageSource = Path.GetFullPath("../../Resources/shared.png"),
                    Name        = project.Name
                });
            }
        }

        private void ShowInvitations()
        {
            var invitations = _projectService.GetInvitations().ToList();

            foreach (var i in invitations)
            {
                _invitations.Add(i);
            }

            InvitationsListView.Visibility = _invitations.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ProjectsListView.SelectedIndex;

            if (selectedIndex == -1)
            {
                PagesFrame.Content = null;
                return;
            }

            PagesFrame.NavigationService.RemoveBackEntry();

            switch (selectedIndex)
            {
                case 0:
                    PagesFrame.Content = new InboxPage(_userId, _itemService, _tagService);
                    break;

                case 1:
                    PagesFrame.Content = new TodayPage(_userId, _itemService, _tagService);
                    break;

                case 2:
                    PagesFrame.Content = new UpcomingPage(this, _userId, _itemService, _tagService);
                    break;

                case 3:
                    PagesFrame.Content = new LogbookPage(this, _userId, _itemService, _tagService);
                    break;

                default:
                    var project = (ProjectView) ProjectsListView.Items[selectedIndex];

                    PagesFrame.Content = new SharedProjectPage(this, project.Id, project.Name, _userId,
                        _itemService, _projectService, _tagService);
                    break;
            }
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this, _userId, _itemService, _tagService);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this, _userId, _itemService, _tagService);
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var enteredTags = SearchTextBox.Text.Split(new[] {' '},
                StringSplitOptions.RemoveEmptyEntries);

            if (enteredTags.Length == 0)
            {
                MessageBox.Show("You need to enter at least one tag to search");
                return;
            }

            FoundToDoItems.ItemsSource = null;

            SearchExpander.IsExpanded          = FillFoundListBox(enteredTags);
            SearchExpander.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private bool FillFoundListBox(IEnumerable<string> tags)
        {
            var foundItems = _tagService.GetItemsByTags(tags).ToList();

            if (foundItems.Count == 0)
                return false;

            FoundToDoItems.ItemsSource = foundItems;

            return true;
        }

        private void FoundToDoItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = FoundToDoItems.SelectedIndex;

            if (index == -1) return;

            var selectedToDoItem = (ToDoItemModel) FoundToDoItems.Items[index];
            var minDate          = DateTime.MinValue.AddYears(1753);

            if (selectedToDoItem.Date == minDate &&
                selectedToDoItem.ProjectName != "" &&
                selectedToDoItem.CompleteDay == minDate)
            {
                var projectView = _projects.First(i => i.Id == (selectedToDoItem.ProjectId ?? -1));

                ProjectsListView.SelectedIndex = _projects.IndexOf(projectView);

                FoundToDoItems.SelectedIndex = -1;
                return;
            }

            // Inbox page.
            if (selectedToDoItem.Date == minDate && selectedToDoItem.ProjectName == "")
                ProjectsListView.SelectedIndex = 0;
            // Today page.
            else if (selectedToDoItem.Date == DateTime.Today)
                ProjectsListView.SelectedIndex = 1;
            // Upcoming page.
            else if (selectedToDoItem.CompleteDay == minDate)
                ProjectsListView.SelectedIndex = 2;
            // Logbook page.
            else
                ProjectsListView.SelectedIndex = 3;

            FoundToDoItems.SelectedIndex = -1;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            if (!_isLogOut)
                _parent.Close();
        }

        private void SupportButton_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://send.monobank.com.ua/nzpHJn4A");
        }
        
        private void LogOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["UserId"].Value = "-1";

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            _isLogOut = true;
            Close();
            _parent.Show();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            _tagService.Refresh();

            _tagService.RefreshRepositories();
            _projectService.RefreshRepositories();
            _itemService.RefreshRepositories();

            PagesListView_OnSelectionChanged(null, null);

            ShowInvitations();
        }

        private void AddSharedProjectButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewProjectStackPanel.Visibility = Visibility.Visible;
            AddProjectButton.Visibility     = Visibility.Collapsed;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                MessageBox.Show("You need to fill project name field");
                return;
            }

            var userLogins = InvitedUsersTextBox.Text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var projectId = _projectService.InviteUsers(new ProjectModel {Name = ProjectNameTextBox.Text}, 
                userLogins, true);

            if (projectId == -1)
                return;

            _projects.Add(new ProjectView
            {
                Id          = projectId,
                ImageSource = Path.GetFullPath("../../Resources/shared.png"),
                Name        = ProjectNameTextBox.Text
            });

            CancelButton_OnClick(null, null);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewProjectStackPanel.Visibility = Visibility.Collapsed;
            AddProjectButton.Visibility     = Visibility.Visible;

            ProjectNameTextBox.Text  = "";
            InvitedUsersTextBox.Text = "";
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            var item       = ((FrameworkElement) sender).DataContext;
            var invitation = _invitations[InvitationsListView.Items.IndexOf(item)];

            if (_projectService.AcceptInvitation(invitation))
            {
                _invitations.Remove(invitation);

                if (_invitations.Count == 0)
                    InvitationsListView.Visibility = Visibility.Collapsed;

                _projects.Add(new ProjectView
                {
                    Id          = invitation.ProjectId,
                    Name        = invitation.ProjectName,
                    ImageSource = Path.GetFullPath("../../Resources/shared.png")
                });
            }
            else
                MessageBox.Show("Can't accept invitation");
        }

        private void DeclineButton_OnClick(object sender, RoutedEventArgs e)
        {
            var item       = ((FrameworkElement) sender).DataContext;
            var invitation = _invitations[InvitationsListView.Items.IndexOf(item)];

            if (_projectService.DeclineInvitation(invitation))
            {
                _invitations.Remove(invitation);

                if (_invitations.Count == 0)
                    InvitationsListView.Visibility = Visibility.Collapsed;
            }
            else
                MessageBox.Show("Can't decline invitation");
        }

        public void RemoveProject()
        {
            _projects.RemoveAt(ProjectsListView.SelectedIndex);
        }
    }
}