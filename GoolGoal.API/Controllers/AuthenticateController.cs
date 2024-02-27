using GoolGoal.API.Auth;
using GoolGoal.API.Common;
using GoolGoal.API.Models;
using GoolGoal.API.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Azure.Core.HttpHeader;

namespace GoolGoal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnv;
    CommonMethod common;

    public AuthenticateController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        Microsoft.AspNetCore.Hosting.IHostingEnvironment env, GoolGoalAppDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _hostingEnv = env;
        common = new CommonMethod(dbContext);
    }


    /// <summary>
    /// Validate User’s Credentials and Return Token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     {        
    ///       "Username": "manisha.avidclan@gmail.com",
    ///       "Password": "Test@123"       
    ///     }
    /// </remarks>
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var token = GetToken(authClaims);
                    var refreshToken = GenerateRefreshToken();

                    int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

                    await _userManager.UpdateAsync(user);

                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = new
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            RefreshToken = refreshToken,
                            Expiration = token.ValidTo,
                            RefreshTokenExpiration = user.RefreshTokenExpiryTime.ToUniversalTime(),
                            Id = user.Id,
                        }
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Please enter valid credentials" });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User does not exists with this email address" });
            }

        }
        catch (Exception ex)
        {
            CommonDBHelper.ErrorLog("AuthenticateController - Login", ex.Message, ex.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong" });
        }
    }

    /// <summary>
    /// New User Registration.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     Email:manisha.avidclan@gmail.com
    ///     FirstName:Manisha 
    ///     LastName:Parmar    
    ///     Password:Test@123
    ///     
    /// </remarks>
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        try
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "User already exists with this email address" });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            if (model.ProfileImage != null)
            {
                if (model.ProfileImage.Length > 0)
                {
                    user.ProfileUrl = await common.UploadBlobFile(model.ProfileImage);
                    //IFormFile formFile = model.ProfileImage;

                    //var folderPath = Path.Combine(_hostingEnv.ContentRootPath, "UserImages");
                    //var folderName = "UserImages";
                    //var fileName = Path.GetRandomFileName() + Path.GetExtension(formFile.FileName).ToLowerInvariant();
                    //var filePath = Path.Combine(folderPath, fileName);

                    //if (!Directory.Exists(folderPath))
                    //    Directory.CreateDirectory(folderPath);

                    //using (var fileStream = new FileStream(filePath, FileMode.Create))
                    //{
                    //    await formFile.CopyToAsync(fileStream);
                    //    fileStream.Flush();
                    //    user.ProfileUrl = fileName;
                    //}
                }
            }
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                string msg = "Password should follow below criteria:\n";
                var strings = result.Errors.ToList().Select(x => x.Description).ToList();
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = msg + string.Join("\n", strings) });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "User created successfully" });
            }
        }
        catch (Exception ex)
        {
            CommonDBHelper.ErrorLog("AuthenticateController - Register", ex.Message, ex.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong" });
        }
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(24),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }

    /// <summary>
    /// Generate a New Access Token
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     {
    ///         "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdEBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBjYTg5ZjU5LWQ5OGYtNGIyMC04ODc2LWI5MmVhMGZiNzBiMiIsImp0aSI6ImZkODZmMTIxLWYyYjgtNDZlOS1hNmVmLTFlZWVhNDQwYWQ4YiIsImV4cCI6MTY4MDA4NzA4NCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo0MjAwIn0.3SNEgBgTFgDmOXg-zBPJ9sGKRAFrzPHnhu_Z0_Y6nuw",
    ///         "RefreshToken": "7Hn1cGy2r+/z9fdKIL1j0kZ7MWjV1n7NKnkYJ+bE4MvTfgpl5ORoX7aUQRr+c/C00MhLcYUISjaqi8ofZO7bOw=="
    ///     }
    ///     
    /// </remarks>
    [HttpPost]
    [Route("Refresh-Token")]
    public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
    {
        try
        {
            if (tokenModel is null)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Invalid client request" });
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Invalid access token or refresh token" });
            }

            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Invalid access token or refresh token" });
            }

            var newAccessToken = GetToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Result = new
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    RefreshToken = newRefreshToken,
                    Expiration = newAccessToken.ValidTo
                }
            });
        }
        catch (Exception ex)
        {
            CommonDBHelper.ErrorLog("AuthenticateController - RefreshToken", ex.Message, ex.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    /// <summary>
    /// Regenerate Password of User and Send it to an existing EmailId
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     {
    ///         "UserEmail": "manisha.avidclan@gmail.com"
    ///     }
    ///     
    /// </remarks>
    [HttpPost]
    [Route("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.UserEmail.Trim());
            if (user != null)
            {
                //var UserLoggedIn = user.ToString();
                string newPassword = common.GenerateRandomPassword();
                var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var UpdatedPassword = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);
                if (UpdatedPassword.Succeeded)
                {
                    //var folderPath = Path.Combine(_hostingEnv.ContentRootPath, "Images");
                    //var filePath = Path.Combine(folderPath, "GoolGoalLogo.png");

                    var mailSend = common.SendMail(user.ToString(), newPassword, user.FirstName);
                    if (mailSend.Result == "Mail Sent Successfully")
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "We have sent an updated password to your registered email address. You can use that password and we recommend updating the password after login from the mobile application" });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Mail not sent" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Password does not updated" });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Email address does not exists" });
            }
        }
        catch (Exception ex)
        {
            CommonDBHelper.ErrorLog("UserController - ForgotPassword", ex.Message, ex.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status200OK, Message = "Something Went Wrong" });
        }

    }

    //[HttpPost]
    //[Route("UploadFile")]
    //public async Task<IActionResult> UploadFile([FromForm] UploadFileModel model)
    //{
    //    try
    //    {
    //        if (model.ProfileImage != null)
    //        {
    //            if (model.ProfileImage.Length > 0)
    //            {
    //                IFormFile formFile = model.ProfileImage;

    //                var folderPath = Path.Combine(_hostingEnv.ContentRootPath, "Images");
    //                //var fileName = Path.GetExtension(formFile.FileName);
    //                var filePath = Path.Combine(folderPath, formFile.FileName);

    //                if (!Directory.Exists(folderPath))
    //                    Directory.CreateDirectory(folderPath);

    //                using (var fileStream = new FileStream(filePath, FileMode.Create))
    //                {
    //                    await formFile.CopyToAsync(fileStream);
    //                    fileStream.Flush();
    //                }
    //            }
    //        }
    //        return Ok();
    //    }
    //    catch (Exception ex)
    //    {
    //        CommonDBHelper.ErrorLog("AuthenticateController - UploadFile", ex.Message, ex.StackTrace);
    //        return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = ex.Message.ToString() });
    //    }
    //}
}