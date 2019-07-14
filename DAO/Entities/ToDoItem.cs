using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAO.Entities
{
    public class ToDoItem
    {
        [Key] public int Id { get; set; }
        [Required] public string Header { get; set; }
        [Required] public string Notes { get; set; }
        public string Date { get; set; }
        public string Deadline { get; set; }
        [ForeignKey("UserOf")] public int UserId { get; set; }
        
        public virtual User UserOf { get; set; }
    }
}