using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using ClassLibrary.Classes;

namespace CourseProjectWPF.Classes
{
    public sealed class UpcomingToDoItems
    {
        public string Date { get; set; }
        public string WeekDay { get; set; }
        public ObservableCollection<ToDoItem> ToDoItems { get; set; }

        public UpcomingToDoItems(ref int increaser, SQLiteConnection connection, bool addToDays)
        {
            Date = DateTime.Today.AddDays(increaser).Day.ToString();

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
            // 
            else if (!addToDays)
                Date = DateTime.Now.AddMonths(increaser).ToString("MMMM");
            else
                WeekDay = DateTime.Today.AddDays(increaser).DayOfWeek.ToString();

            ToDoItems = new ObservableCollection<ToDoItem>();

            if (addToDays)
                FillToDoItemsByDays(increaser, connection);
            else
                FillToDoItemsByMonths(increaser, connection);
        }

        private void FillToDoItemsByDays(int dayIncreaser, SQLiteConnection connection)
        {
            const string command = "SELECT * FROM ToDoItems WHERE Date=@date";

            using (var cmd = new SQLiteCommand(command, connection))
            {
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@date",
                    (long) (DateTime.Today.AddDays(dayIncreaser) - DateTime.MinValue).TotalMilliseconds);

                using (var reader = cmd.ExecuteReader())
                {
                    MainWindow.FillCollection(reader, ToDoItems);
                }
            }
        }

        private void FillToDoItemsByMonths(int monthIncreaser, SQLiteConnection connection)
        {
            const string command = "SELECT * FROM ToDoItems WHERE Date BETWEEN @begin AND @end";

            long begin, end;

            // Current month has remaining days.
            if (WeekDay != null)
            {
                var lastMonthDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

                begin = (long) (DateTime.Today.AddDays(8) - DateTime.MinValue).TotalMilliseconds;
                end   = (long) (lastMonthDay - DateTime.MinValue).TotalMilliseconds;
            }
            else
            {
                var beginDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(monthIncreaser);
                var endDate = new DateTime(beginDate.Year, beginDate.Month, 
                    DateTime.DaysInMonth(beginDate.Year, beginDate.Month));

                begin = (long) (beginDate - DateTime.MinValue).TotalMilliseconds;
                end   = (long) (endDate - DateTime.MinValue).TotalMilliseconds;
            }

            using (var cmd = new SQLiteCommand(command, connection))
            {
                cmd.Prepare();

                cmd.Parameters.AddWithValue("@begin", begin);
                cmd.Parameters.AddWithValue("@end", end);

                using (var reader = cmd.ExecuteReader())
                {
                    MainWindow.FillCollection(reader, ToDoItems);
                }
            }
        }
    }
}