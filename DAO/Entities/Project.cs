using System.ComponentModel.DataAnnotations;

namespace DAO.Entities
{
    public class Project
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; }
    }
}