using System.ComponentModel.DataAnnotations;

namespace GoolGoal.API.Auth;

public class RegisterModel
{
    [Required(ErrorMessage = "First Name is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required")]
    public string? LastName { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }

    public IFormFile? ProfileImage { get; set; }
}

//public class UploadFileModel
//{
//    public IFormFile? ProfileImage { get; set; }
//}