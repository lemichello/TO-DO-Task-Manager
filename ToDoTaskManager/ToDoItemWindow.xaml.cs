using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BUS.Models;
using BUS.Services;
using ToDoTaskManager.Classes;

namespace ToDoTaskManager
{
    public partial class ToDoItemWindow
    {
        private readonly DateTime                       _minDate = DateTime.MinValue.AddYears(1753);
        private readonly ObservableCollection<TagModel> _tagsList;
        private readonly TagService                     _tagService;
        private readonly int?                           _projectId;
        public IEnumerable<int> SelectedTagsId { get; set; }

        public ToDoItemView Item { get; }
        public bool ToDelete { get; private set; }

        public ToDoItemWindow(int? projectId)
        {
            InitializeComponent();

            if (projectId != null)
            {
                SharedTagButton.Visibility = Visibility.Visible;
                Height                     = 465;
            }

            _projectId = projectId;

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
            Item                            = new ToDoItemView();
            ToDelete                        = false;

            _tagService = TagService.GetInstance();
            _tagsList   = new ObservableCollection<TagModel>();

            FillTagsCollection();

            TagsListBox.ItemsSource = _tagsList;
        }

        public void ShowDialog(DateTime dateTime)
        {
            PickedDate.SelectedDate = dateTime;

            ShowDialog();
        }

        public ToDoItemWindow(int? projectId, ToDoItemModel item) : this(projectId)
        {
            Item = new ToDoItemView {Id = item.Id};

            HeaderText.Text = item.Header;
            NotesText.Text  = item.Notes;

            if (item.Date != _minDate)
                PickedDate.SelectedDate = item.Date;

            if (item.Deadline != _minDate)
                PickedDeadline.SelectedDate = item.Deadline;

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;

            SelectTags();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HeaderText.Text))
            {
                MessageBox.Show("You need to fill header");
                return;
            }

            Item.Header = HeaderText.Text;
            Item.Notes  = NotesText.Text;

            if (PickedDate.SelectedDate != null)
                Item.Date = (DateTime) PickedDate.SelectedDate;
            else
                Item.Date = _minDate;

            if (PickedDeadline.SelectedDate != null)
                Item.Deadline = (DateTime) PickedDeadline.SelectedDate;
            else
                Item.Deadline = _minDate;

            Item.CompleteDay = _minDate;

            if (Item.Deadline == DateTime.Today)
            {
                Item.DeadlineColor = "Red";
                Item.DeadlineShort = "today";
            }
            else if (Item.Deadline != _minDate)
            {
                var remainingDays = (Item.Deadline - DateTime.Today).TotalDays;

                Item.DeadlineColor = "Gray";
                Item.DeadlineShort = $"{remainingDays}d left";
            }

            SelectedTagsId = GetTagsId();

            DialogResult = true;
            Close();
        }

        private IEnumerable<int> GetTagsId()
        {
            return from TagModel i in TagsListBox.SelectedItems select i.Id;
        }

        private void DeleteTaskButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this task?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            ToDelete = true;

            Close();
        }

        private void FillTagsCollection()
        {
            // If projectId is null, getting only tags, that don't belong to any project.
            var collection = _projectId != null
                ? _tagService.Get(i => i.ProjectId == _projectId || i.ProjectId == null).ToList()
                : _tagService.Get(i => i.ProjectId == null).ToList();

            foreach (var i in collection)
            {
                _tagsList.Add(i);
            }
        }

        private void SelectTags()
        {
            var collection = _tagService.GetSelected(Item.Id).ToList();

            foreach (var i in collection)
            {
                TagsListBox.SelectedItems.Add(i);
            }
        }

        private void TagsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagsListBox.SelectedIndex == -1)
            {
                EditButton.Visibility   = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;

                Height = 420;
            }
            else
            {
                EditButton.Visibility   = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;

                Height = 555;
            }
        }

        private void AddTagButton_OnCLick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            var tag = new TagModel
            {
                Text         = window.NewText,
                TagTextColor = "Black"
            };

            _tagService.Add(tag);
            _tagsList.Add(tag);
        }

        private void AddSharedTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            var tag = new TagModel
            {
                Text         = window.NewText,
                ProjectId    = _projectId,
                TagTextColor = "#2295F2"
            };

            _tagService.Add(tag);
            _tagsList.Add(tag);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();
            var tag    = (TagModel) TagsListBox.SelectedItems[TagsListBox.SelectedItems.Count - 1];

            if (window.ShowDialog() == false) return;

            tag.Text = window.NewText;

            _tagsList[TagsListBox.SelectedIndex].Text = tag.Text;
            _tagService.Update(tag);
        }

        private void DeleteTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this tag? This will delete all " +
                                "occurrences of this tag in tasks.", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var tag = _tagsList[TagsListBox.SelectedIndex];

            if (_tagService.Remove(tag))
                _tagsList.Remove(tag);
        }
    }
}