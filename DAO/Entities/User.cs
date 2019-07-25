using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAO.Entities
{
    public class User
    {
        [Key] public int Id { get; set; }
        [Required] public string Login { get; set; }
        [Required] public string Password { get; set; }

        public virtual ICollection<ToDoItem> ToDoItems { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
}