using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BUS.Models;
using BUS.Services;

namespace CourseProjectWPF
{
    public partial class ToDoItemWindow : Window
    {
        private readonly ObservableCollection<TagModel> _tagsList;
        private readonly TagService                     _tagService;
        public IEnumerable<int> SelectedTagsId { get; set; }

        public ToDoItemModel Item { get; }
        public bool ToDelete { get; private set; }

        public ToDoItemWindow(int userId)
        {
            InitializeComponent();

            PickedDate.DisplayDateStart     = DateTime.Now;
            PickedDeadline.DisplayDateStart = DateTime.Now;
            Item                            = new ToDoItemModel();
            ToDelete                        = false;

            _tagService       = new TagService(userId);
            _tagsList         = new ObservableCollection<TagModel>();

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
            Item = new ToDoItemModel {Id = item.Id};

            HeaderText.Text = item.Header;
            NotesText.Text  = item.Notes;

            if (item.Date != DateTime.MinValue.AddYears(1753))
                PickedDate.SelectedDate = item.Date;

            if (item.Deadline != DateTime.MinValue.AddYears(1753))
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
                Item.Date = DateTime.MinValue.AddYears(1753);

            if (PickedDeadline.SelectedDate != null)
                Item.Deadline = (DateTime) PickedDeadline.SelectedDate;
            else
                Item.Deadline = DateTime.MinValue.AddYears(1753);

            Item.CompleteDay = DateTime.MinValue.AddYears(1753);

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
            var tag    = _tagsList[TagsListBox.SelectedIndex];

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