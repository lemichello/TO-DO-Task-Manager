using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BUS.Models;

namespace ToDoTaskManager.Classes
{
    public sealed class UpcomingToDoItems
    {
        public string Date { get; set; }
        public string WeekDay { get; set; }
        public ObservableCollection<ToDoItemView> ToDoItems { get; set; }

        private readonly List<ToDoItemModel> _allItems;

        public UpcomingToDoItems(ref int increaser, List<ToDoItemModel> items, bool addToDays)
        {
            _allItems = items;
            Date      = DateTime.Today.AddDays(increaser).Day.ToString();

            if (increaser == 0 && !addToDays)
            {
                Date = DateTime.Now.AddDays(8).ToString("MMMM");
                WeekDay =
                    $"{DateTime.Now.AddDays(8).Day}-{DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)}";
                increaser++;
            }
            else if (!addToDays)
                Date = DateTime.Now.AddMonths(increaser).ToString("MMMM");
            else
                WeekDay = DateTime.Today.AddDays(increaser).DayOfWeek.ToString();

            ToDoItems = new ObservableCollection<ToDoItemView>();

            if (addToDays)
                FillToDoItemsByDays(increaser);
            else
                FillToDoItemsByMonths(increaser);
        }
        
        // Fill remaining items.
        public UpcomingToDoItems(int year, List<ToDoItemModel> items)
        {
            ToDoItems = new ObservableCollection<ToDoItemView>();
            Date = year.ToString();

            AddItems(items);

            foreach (var i in ToDoItems)
            {
                var day   = i.Date.Day > 9 ? i.Date.Day.ToString() : $"0{i.Date.Day}";
                var month = i.Date.Month > 9 ? i.Date.Month.ToString() : $"0{i.Date.Month}";
                
                i.ShortDate = $"{day}.{month}";
                i.DateVisibility = "Visible";
            }
        }

        private void AddItems(List<ToDoItemModel> items)
        {
            foreach (var i in items)
            {
                var itemView = new ToDoItemView
                {
                    Id          = i.Id,
                    Header      = i.Header,
                    Notes       = i.Notes,
                    Date        = i.Date,
                    Deadline    = i.Deadline,
                    CompleteDay = i.CompleteDay,
                    ProjectId   = i.ProjectId,
                    ProjectName = i.ProjectName,
                    Timer       = i.Timer
                };

                if (i.Deadline == DateTime.MinValue.AddYears(1753))
                {
                    ToDoItems.Add(itemView);
                    continue;
                }
                
                if (i.Deadline <= DateTime.Today)
                {
                    itemView.DeadlineColor = "Red";
                    itemView.DeadlineShort = "today";
                }
                else
                {
                    var remainingDays = (i.Deadline - DateTime.Today).TotalDays;
                    
                    itemView.DeadlineColor = "Gray";
                    itemView.DeadlineShort = $"{remainingDays}d left";
                }
                
                ToDoItems.Add(itemView);
            }
        }

        
        private void FillToDoItemsByDays(int dayIncreaser)
        {
            var items = _allItems.Where(i =>
                i.Date == DateTime.Today.AddDays(dayIncreaser)).ToList();

            AddItems(items);

            foreach (var i in ToDoItems)
            {
                i.DateVisibility = "Collapsed";
            }
        }

        private void FillToDoItemsByMonths(int monthIncreaser)
        {
            DateTime begin, end;

            // Current month has remaining days.
            if (WeekDay != null)
                FillByRemainingDays(out begin, out end);
            else
                FillByNextMonth(monthIncreaser, out begin, out end);

            var items = _allItems.Where(i => i.Date >= begin && i.Date <= end).ToList();

            AddItems(items);

            foreach (var i in ToDoItems)
            {
                var day = i.Date.Day > 9 ? i.Date.Day.ToString() : $"0{i.Date.Day}";
                var month = i.Date.Month > 9 ? i.Date.Month.ToString() : $"0{i.Date.Month}";
                
                i.ShortDate = $"{day}.{month}";
                i.DateVisibility = "Visible";
            }
        }

        private static void FillByNextMonth(int monthIncreaser, out DateTime begin, out DateTime end)
        {
            begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(monthIncreaser);
            end = new DateTime(begin.Year, begin.Month,
                DateTime.DaysInMonth(begin.Year, begin.Month));
        }

        private static void FillByRemainingDays(out DateTime begin, out DateTime end)
        {
            begin = DateTime.Today.AddDays(8);

            var lastMonthDay = new DateTime(begin.Year, begin.Month,
                DateTime.DaysInMonth(begin.Year, begin.Month));

            end = lastMonthDay;
        }
    }
}