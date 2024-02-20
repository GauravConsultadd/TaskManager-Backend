using TaskManager.Data;
using TaskManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManager.Dtos;

namespace TaskManager.Controller {
    [ApiController]
    [Route("api/[controller]")]
    public class UserController: ControllerBase {
        private readonly AppDbContext _appDbContext;

        public UserController(AppDbContext appDbContext) {
            this._appDbContext=appDbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddUser([FromBody] User user) {
            user.HashPassword();
            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginUser) {
            try {
                var user = await _appDbContext.Users.FirstOrDefaultAsync(u=> u.Email==loginUser.Email);
                if(user == null) {
                    return BadRequest("Invalid Credentials");
                }
                
                bool passwordMatched = loginUser.VerifyPassword(user.Password); 

                if(!passwordMatched) {
                    return BadRequest("Invalid credentials");
                }

                string token = loginUser.GenerateToken(loginUser.Email,user.Role);

                return Ok( new {token});
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                Console.WriteLine(err);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [HttpGet("currentUser")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> GetCurrentUser() {
            try {
                var emailClaim = HttpContext.User.FindFirst(ClaimTypes.Email);
                if (emailClaim == null)
                {
                    return NotFound("user not found");
                }

                var email = emailClaim.Value;
                // You can fetch additional user information from your database or other sources
                var user = await _appDbContext.Users.FirstOrDefaultAsync(u=> u.Email==email);
                return Ok(user);
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id,[FromBody]UpdateUserDto updateUserDto) {
            try {
                var user = await _appDbContext.Users.FirstOrDefaultAsync((u)=> u.id==id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties if they are not null
                if (updateUserDto.Name != null)
                {
                    user.Name = updateUserDto.Name;
                }
                if (updateUserDto.Email != null)
                {
                    user.Email = updateUserDto.Email;
                }
                if(updateUserDto.Role != null) {
                    if(updateUserDto.Role=="user" || updateUserDto.Role=="admin")
                        user.Role = updateUserDto.Role;
                    else return BadRequest("invalid role assignment");
                }
                // Update other properties as needed

                // Save changes to the database
                await _appDbContext.SaveChangesAsync();

                return Ok(user);
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto) {
            try {
                var user = await _appDbContext.Users.FirstOrDefaultAsync((u)=> u.Email==forgotPasswordDto.Email);
                if (user == null) return BadRequest("User not found");

                var token = Guid.NewGuid().ToString();
                var expirationDate = DateTime.UtcNow.AddMinutes(10); // Token expiration time

                // Store token in the database
                user.ResetPasswordToken = token;
                user.ResetPasswordExpiration = expirationDate;

                try {
                    await _appDbContext.SaveChangesAsync();

                    var callbackUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/User/resetPassword?userId={user.id}&token={token}";

                    var emailService = new EmailService();
                    await emailService.SendEmailAsync(user.Email,"Reset Password Subject","Click the link for reset password token "+callbackUrl);

                    return Ok(new {url=callbackUrl});
                }
                catch(Exception err) {
                    user.ResetPasswordToken = null;
                    Console.WriteLine("error occured: "+err.Message);
                    return StatusCode(500,"INTERNAL SERVER ERROR");
                }
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        [HttpPost("resetPassword/{id}")]
        public async Task<IActionResult> ResetPassword(int id,[FromBody] ResetPasswordDto resetPasswordDto) {
            try {
                var user = await _appDbContext.Users.FirstOrDefaultAsync((u)=> u.id==id);
                if(user==null) {
                    return BadRequest("User not found");
                }

                string? token = resetPasswordDto.Token;

                if(token != user.ResetPasswordToken || user.ResetPasswordExpiration < DateTime.UtcNow) {
                    return BadRequest("token is expired or invalid");
                }

                user.Password = resetPasswordDto.Password;
                user.HashPassword();
                user.ResetPasswordToken = null;
                await _appDbContext.SaveChangesAsync();

                return Ok("Password changed");
            }
            catch(Exception err) {
                Console.WriteLine("error occured: "+err.Message);
                return StatusCode(500,"INTERNAL SERVER ERROR");
            }
        }

        
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllUsers() {
            var users = await _appDbContext.Users.ToListAsync();

            return Ok(users);
        }

    }
}