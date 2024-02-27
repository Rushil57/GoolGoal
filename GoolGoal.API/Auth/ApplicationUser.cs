using Microsoft.AspNetCore.Identity;

namespace GoolGoal.API.Auth;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileUrl { get; set;}
}