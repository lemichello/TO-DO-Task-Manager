using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAO.Entities
{
    public class ProjectsUsers
    {
        [Key, Column(Order = 1), ForeignKey("ProjectOf")]
        public int ProjectId { get; set; }

        [Key, Column(Order = 2), ForeignKey("UserOf")]
        public int UserId { get; set; }

        [ForeignKey("InviterOf")] public int InviterId { get; set; }

        public bool IsAccepted { get; set; }

        public virtual Project ProjectOf { get; set; }
        public virtual User UserOf { get; set; }
        public virtual User InviterOf { get; set; }
    }
}