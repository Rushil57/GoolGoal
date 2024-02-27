using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoolGoal.API.Models
{
    public class StoreAPIResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string FixtureId { get; set; }
        [Required]
        public string APIName { get; set; }
        [Required]
        public string APIResponse { get; set; }
        [Required]
        public DateTime CreatedDateTime { get; set; }
    }
}
