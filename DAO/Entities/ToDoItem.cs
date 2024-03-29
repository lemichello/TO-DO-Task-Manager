using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAO.Entities
{
    public class ToDoItem
    {
        [Key] public int Id { get; set; }
        [Required] public string Header { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CompleteDate { get; set; }
        [ForeignKey("ProjectOf")]public int? ProjectId { get; set; }
        [ForeignKey("UserOf")] public int UserId { get; set; }

        public virtual User UserOf { get; set; }
        public virtual Project ProjectOf { get; set; }
    }
}