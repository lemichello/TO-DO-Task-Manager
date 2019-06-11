using System;

namespace ClassLibrary.Classes
{
    public sealed class ToDoItem
    {
        public long Id { get; set; }
        public string Header { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
        public DateTime Deadline { get; set; }
        public string CompleteDay { get; set; }
    }
}