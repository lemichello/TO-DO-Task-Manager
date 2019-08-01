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
    public partial class ToDoItemWindow : Window
    {
        private readonly DateTime                       _minDate = DateTime.MinValue.AddYears(1753);
        private readonly ObservableCollection<TagModel> _tagsList;
        private readonly TagService                     _tagService;
        public IEnumerable<int> SelectedTagsId { get; set; }

        public ToDoItemView Item { get; }
        public bool ToDelete { get; private set; }

        public ToDoItemWindow(int userId)
        {
            InitializeComponent();

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
            Item                            = new ToDoItemView();
            ToDelete                        = false;

            _tagService = new TagService(userId);
            _tagsList   = new ObservableCollection<TagModel>();

            FillTagsCollection();

            TagsListBox.ItemsSource = _tagsList;
        }

        public void ShowDialog(DateTime dateTime)
        {
            PickedDate.SelectedDate = dateTime;

            ShowDialog();
        }

        public ToDoItemWindow(ToDoItemModel item, int userId) : this(userId)
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

        // Delete TO-DO item.
        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            ToDelete = true;

            Close();
        }

        private void FillTagsCollection()
        {
            // Getting all tags.
            var collection = _tagService.Get(i => true).ToList();

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
                EditButton.Visibility   = Visibility.Hidden;
                DeleteButton.Visibility = Visibility.Hidden;
            }
            else
            {
                EditButton.Visibility   = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

        private void AddButton_OnCLick(object sender, RoutedEventArgs e)
        {
            var window = new TagWindow();

            if (window.ShowDialog() == false) return;

            var tag = new TagModel
            {
                Text = window.NewText,
            };

            _tagService.Add(tag);
            _tagsList.Add(tag);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TagsListBox.SelectedItems.Count == 0) return;

            var window = new TagWindow();
            var tag    = (TagModel) TagsListBox.SelectedItems[TagsListBox.SelectedItems.Count - 1];

            if (window.ShowDialog() == false) return;

            tag.Text = window.NewText;

            _tagsList[TagsListBox.SelectedIndex].Text = tag.Text;
            _tagService.Update(tag);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TagsListBox.SelectedItems.Count == 0) return;

            var tag = _tagsList[TagsListBox.SelectedIndex];

            if (_tagService.Remove(tag))
                _tagsList.Remove(tag);
        }
    }
}