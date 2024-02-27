using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoolGoal.API.Models
{
    public class TeamFavourite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string TeamId { get; set; }
        [Required]
        public Boolean IsFavourite { get; set; }
        [Required]
        public DateTime CreatedDateTime { get; set; }
    }
}
