namespace ToDoTaskManager.Classes
{
    public class ProjectView
    {
        public int Id { get; set; }
        public string ImageSource { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = true;
    }
}