using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAO.Entities
{
    public class Tag
    {
        [Key] public int Id { get; set; }
        [Required] public string Text { get; set; }
        [ForeignKey("UserOf")] public int UserId { get; set; }
        
        public virtual User UserOf { get; set; }
    }
}