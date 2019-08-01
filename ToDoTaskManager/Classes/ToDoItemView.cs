using BUS.Models;

namespace ToDoTaskManager.Classes
{
    public class ToDoItemView : ToDoItemModel
    {
        public string DeadlineShort { get; set; }
        public string ShortDate { get; set; }
        public string DeadlineColor { get; set; }
        public string DateVisibility { get; set; }
    }
}