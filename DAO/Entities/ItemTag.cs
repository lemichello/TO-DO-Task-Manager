using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAO.Entities
{
    public class ItemTag
    {
        [Key, Column(Order = 1), ForeignKey("ItemOf")]
        public int ItemId { get; set; }

        [Key, Column(Order = 2), ForeignKey("TagOf")]
        public int TagId { get; set; }

        public virtual ToDoItem ItemOf { get; set; }
        public virtual Tag TagOf { get; set; }
    }
}