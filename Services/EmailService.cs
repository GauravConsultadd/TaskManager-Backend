using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http.HttpResults;


public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService()
    {
        _smtpClient = new SmtpClient(Environment.GetEnvironmentVariable("EMAIL_HOST"), 587)
        {
            Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("EMAIL_HOST_USER"), Environment.GetEnvironmentVariable("EMAIL_HOST_PASSWORD")),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string? to, string subject, string body)
    {
        string emailId = Environment.GetEnvironmentVariable("EMAIL_HOST_USER");

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailId),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        if(to != null) mailMessage.To.Add(to);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}