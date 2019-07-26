using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAO.Entities
{
    public class Project
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; }
        public virtual ICollection<ToDoItem> Items { get; set; }
    }
}