using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BUS.Models;
using BUS.Services;

namespace CourseProjectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly int _userId;
        private readonly TagService _service;

        public MainWindow()
        {
            InitializeComponent();

            _userId = 1;
            _service = new TagService(_userId);
        }

        private void PagesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PagesListView.SelectedIndex == -1)
            {
                PagesFrame.Content = null;
                return;
            }

            switch (PagesListView.SelectedIndex)
            {
                case 0:
                    PagesFrame.Content = new InboxPage();
                    break;

                case 1:
                    PagesFrame.Content = new TodayPage();
                    break;

                case 2:
                    PagesFrame.Content = new UpcomingPage(this);
                    break;

                case 3:
                    PagesFrame.Content = new LogbookPage(this);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void UpdateUpcomingPage()
        {
            PagesFrame.Content = new UpcomingPage(this);
        }

        public void UpdateLogbookPage()
        {
            PagesFrame.Content = new LogbookPage(this);
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
            var foundItems = _service.GetItemsByTags(tags).ToList();

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

            if (selectedToDoItem.Date == DateTime.MinValue.AddYears(1753))
                PagesListView.SelectedIndex = 0;

            else if (selectedToDoItem.Date == DateTime.Today)
                PagesListView.SelectedIndex = 1;

            else if (selectedToDoItem.CompleteDay == DateTime.MinValue.AddYears(1753))
                PagesListView.SelectedIndex = 2;
            else
                PagesListView.SelectedIndex = 3;
        }
    }
}