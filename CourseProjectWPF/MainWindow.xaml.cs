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
        private readonly LoginWindow                       _parent;
        private readonly int                               _userId;
        private readonly TagService                        _tagService;
        private readonly ProjectService                    _projectService;
        private readonly ObservableCollection<ProjectView> _projects;
        private          bool                              _isLogOut;

        public MainWindow(LoginWindow parent, int userId)
        {
            InitializeComponent();

            _projectService = new ProjectService(userId);
            _projects       = new ObservableCollection<ProjectView>();

            ShowDefaultProjects();
            ShowSharedProjects();

            ProjectsListView.ItemsSource = _projects;

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
                Name = "Inbox"
            });

            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/today.png",
                Name = "Today"
            });

            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/upcoming.png",
                Name = "Upcoming"
            });

            _projects.Add(new ProjectView
            {
                ImageSource = "Resources/logbook.png",
                Name = "Logbook"
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
                    Name = project.Name
                });
            }
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ProjectsListView.SelectedIndex;

            if (selectedIndex == -1)
            {
                PagesFrame.Content = null;
                return;
            }

            switch (selectedIndex)
            {
                case 0:
                    PagesFrame.Content = new InboxPage(_userId);
                    break;

                case 1:
                    PagesFrame.Content = new TodayPage(_userId);
                    break;

                case 2:
                    PagesFrame.Content = new UpcomingPage(this, _userId);
                    break;

                case 3:
                    PagesFrame.Content = new LogbookPage(this, _userId);
                    break;

                default:
                    var project = ((ProjectView) ProjectsListView.Items[selectedIndex]);

                    PagesFrame.Content = new SharedProjectPage(project.Id,project.Name, _userId);
                    break;
            }
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this, _userId);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this, _userId);
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

            // Inbox page.
            if (selectedToDoItem.Date == minDate)
                ProjectsListView.SelectedIndex = 0;
            // Today page.
            else if (selectedToDoItem.Date == DateTime.Today)
                ProjectsListView.SelectedIndex = 1;
            // Upcoming page.
            else if (selectedToDoItem.CompleteDay == minDate)
                ProjectsListView.SelectedIndex = 2;
            // Logbook page.
            else if (selectedToDoItem.ProjectName == "")
                ProjectsListView.SelectedIndex = 3;
            // Project page.
            else
            {
                var foundProjects = _projectService.GetProjects().ToList();
                var projectIndex = foundProjects
                    .IndexOf(foundProjects.First(i => i.Name == selectedToDoItem.ProjectName));

                ProjectsListView.SelectedIndex = projectIndex;
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            if (!_isLogOut)
                _parent.Close();
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

            PagesListView_OnSelectionChanged(null, null);
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
            var projectId  = _projectService.AddProject(new ProjectModel {Name = ProjectNameTextBox.Text}, userLogins);

            if (projectId == -1)
                return;

            _projects.Add(new ProjectView
            {
                Id          = projectId,
                ImageSource = Path.GetFullPath("../../Resources/shared.png"),
                Name = ProjectNameTextBox.Text
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
    }
}