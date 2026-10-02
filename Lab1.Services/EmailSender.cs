using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Lab1.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string subject, string message)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);

        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"];

        var recipientEmail = _configuration["EmailSettings:RecipientEmail"];

        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        var emailMessage = new MimeMessage();

        // Sender
        emailMessage.From.Add(
            new MailboxAddress(senderName, senderEmail));

        // Recipient
        emailMessage.To.Add(
            MailboxAddress.Parse(recipientEmail));

        // Subject
        emailMessage.Subject = subject;

        // Message body
        emailMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(
            smtpHost,
            smtpPort,
            SecureSocketOptions.StartTls);

        await smtpClient.AuthenticateAsync(
            username,
            password);

        await smtpClient.SendAsync(emailMessage);

        await smtpClient.DisconnectAsync(true);
    }
}