using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BUS.Models;
using BUS.Services;

namespace CourseProjectWPF.Classes
{
    public sealed class UpcomingToDoItems
    {
        public string Date { get; set; }
        public string WeekDay { get; set; }
        public ObservableCollection<ToDoItemModel> ToDoItems { get; set; }

        private readonly List<ToDoItemModel> _allItems;

        public UpcomingToDoItems(ref int increaser, ToDoItemService service, bool addToDays)
        {
            _allItems = service.Get(i => i.CompleteDay == DateTime.MinValue.AddYears(1753)).ToList();
            Date     = DateTime.Today.AddDays(increaser).Day.ToString();

            if (increaser == 0 && !addToDays)
            {
                // Show remaining days of current month, e.g. 17-31 October.
                if (DateTime.Now.AddDays(8).Month == DateTime.Now.Month)
                {
                    Date = DateTime.Now.ToString("MMMM");
                    WeekDay =
                        $"{DateTime.Now.AddDays(8).Day}-{DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)}";
                }
                // There's no remaining days for current month.
                else
                {
                    // Getting next name of the next month and increasing counter.
                    Date = DateTime.Now.AddDays(8).ToString("MMMM");
                    increaser++;
                }
            }
            else if (!addToDays)
                Date = DateTime.Now.AddMonths(increaser).ToString("MMMM");
            else
                WeekDay = DateTime.Today.AddDays(increaser).DayOfWeek.ToString();

            ToDoItems = new ObservableCollection<ToDoItemModel>();

            if (addToDays)
                FillToDoItemsByDays(increaser);
            else
                FillToDoItemsByMonths(increaser);
        }

        private void FillToDoItemsByDays(int dayIncreaser)
        {
            var items = _allItems.Where(i =>
                i.Date == DateTime.Today.AddDays(dayIncreaser)).ToList();

            foreach (var i in items)
            {
                ToDoItems.Add(i);
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

            foreach (var i in items)
            {
                ToDoItems.Add(i);
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
            var lastMonthDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            begin = DateTime.Today.AddDays(8);
            end   = lastMonthDay;
        }
    }
}