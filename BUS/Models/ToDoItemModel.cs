using System;
using System.Windows.Threading;

namespace BUS.Models
{
    public class ToDoItemModel
    {
        public long Id { get; set; }
        public string Header { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CompleteDay { get; set; }
        public DispatcherTimer Timer { get; set; }
    }
}