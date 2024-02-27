using System.ComponentModel.DataAnnotations;

namespace GoolGoal.API.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "Id is required")]
        public string? Id { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }
        public IFormFile? ProfileImage { get; set; } 
    }
}
