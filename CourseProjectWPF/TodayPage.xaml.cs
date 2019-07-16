﻿using System;
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
    /// Interaction logic for TodayPage.xaml
    /// </summary>
    public partial class TodayPage : Page
    {
        private readonly ObservableCollection<ToDoItemModel> _toDoItemsCollection;
        private readonly TodayToDoItemOperations             _toDoItemOperations;
        private readonly ToDoItemService                     _service;
        private readonly int _userId;

        public TodayPage()
        {
            InitializeComponent();

            _userId = 1;
            _toDoItemsCollection = new ObservableCollection<ToDoItemModel>();
            _service = new ToDoItemService(_userId);

            FillCollection();

            ToDoItemsListView.ItemsSource = _toDoItemsCollection;
            _toDoItemOperations =
                new TodayToDoItemOperations(ToDoItemsListView, _toDoItemsCollection, _userId);
        }

        private void FillCollection()
        {
            var collection = _service.Get(item => item.Date == DateTime.Today &&
                                                  item.CompleteDay == DateTime.MinValue.AddYears(1753)).ToList();

            foreach (var i in collection)
            {
                _toDoItemsCollection.Add(i);
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Add();
        }

        private void ToDoItemsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _toDoItemOperations.Selected();
        }

        private void ToDoItem_OnChecked(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Checked(sender);
        }

        private void ToDoItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _toDoItemOperations.Unchecked(sender);
        }
    }
}