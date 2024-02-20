using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace TaskManager.Models {
    public class User {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id {get;set;}

        [Required]
        [MaxLength(50)]
        public string? Name {get;set;}

        [Required]
        [MaxLength(50)]
        public string? Email {get;set;}

        [Required]
        public string? Password {get;set;}
        public string? Role {get;set;}
        public string? ResetPasswordToken {get;set;}
        public DateTime? ResetPasswordExpiration {get;set;}

        public ICollection<Tasks>? CreatedTasks { get; set; }
        public ICollection<Tasks>? AssignedTasks { get; set; }

        public User() {
            CreatedTasks = new List<Tasks>();
            AssignedTasks = new List<Tasks>();
        }

        public void HashPassword()
        {
            Console.WriteLine("this is password"+this.Password);
            Password = BCrypt.Net.BCrypt.HashPassword(this.Password);
        }
    }

        public class LoginModel {
            [Required]
            [MaxLength(50)]
            public string? Email {get;set;}

            [Required]
            public string? Password {get;set;}

            public bool VerifyPassword(string? Password)
            {
                return BCrypt.Net.BCrypt.Verify(this.Password, Password);
            }

            public string GenerateToken(string? Email,string? Role)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                string? secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                if (secretKey == null || Email == null) {
                    return "Nothing";
                }
                var key = Encoding.ASCII.GetBytes(secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Email, Email),
                        new Claim(ClaimTypes.Role, Role) // Add role claim
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"), // Set the audience claim
                    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
        }
}