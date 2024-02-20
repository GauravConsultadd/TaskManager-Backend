namespace TaskManager.Dtos
{
    public class UpdateUserDto
    {
        public string? Name {get;set;}
        public string? Email { get; set; }
        public string? Role {get;set;}
    }

    public class ForgotPasswordDto {
        public string? Email {get;set;}
    }

    public class ResetPasswordDto {
        public string? Password {get;set;}
        public string? Token {get;set;}
    }

}