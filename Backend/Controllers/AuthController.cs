using Backend.Data;
using Backend.Entities;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string connectionString;
        private readonly IConfiguration configuration;
        private readonly DataBaseHelper dataBaseHelper;

        public AuthController(IConfiguration _configuration)
        {
            configuration = _configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
            dataBaseHelper = new DataBaseHelper(_configuration);
        }

        public static User staticUser = new();
        [HttpPost("register")]

        public async Task<ActionResult<User>> RegisterUser(UserDto request)
        {
            var hashedPassword = new PasswordHasher<User>().HashPassword(staticUser, request.PasswordHash);
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hashedPassword,
                Role = request.Role
            };

            //now implemeting the database logic
            var(success, ErrorMessage) = await dataBaseHelper.ToRegisterUser(newUser);
            if (success)
            {
                return Ok(new {message = ErrorMessage});
            }else { return BadRequest(new { message = ErrorMessage }); }

        }

        
        [HttpPost("login")]
        
        public async Task<ActionResult<string>> Login(LoginUser request)
        {
            var user = dataBaseHelper.GetUserByUsername(request.Username);
            if(user == null)
            {
                return BadRequest(new{message = "User doesnot exsist"});
            }
            if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.PasswordHash) == PasswordVerificationResult.Failed)
            {
                return BadRequest(new {message = "Wrong Password"});
            }
            return Ok(new
            {
                token = CreateToken(user),
                username = user.Username,
                role = user.Role
            });
        }

        //Making token for the login

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescription = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescription);
        }
    }
}
