using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoolGoal.API.Models
{
    public class LeagueFollow 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string LeagueId { get; set; }
        [Required]
        public Boolean IsFollow { get; set; }
        [Required]
        public DateTime CreatedDateTime { get; set; }
    }
}
