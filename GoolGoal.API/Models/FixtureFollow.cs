using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoolGoal.API.Models
{
    public class FixtureFollow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string FixtureId { get; set; }
        [Required]
        public Boolean IsFollow { get; set; }
        [Required]
        public DateTime CreatedDateTime { get; set; }
    }
}
