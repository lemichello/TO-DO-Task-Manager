using BUS.Models;

namespace CourseProjectWPF.Classes
{
    public class ToDoItemView : ToDoItemModel
    {
        public string DeadlineShort { get; set; }
        public string ShortDate { get; set; }
        public string DeadlineColor { get; set; }
    }
}