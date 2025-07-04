using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.OpenApi.Models;
using MimeKit;

namespace Fichte.Providers
{
    public class SmtpProvider
    {

        struct EmailSettings
        {
            public string SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public string SmtpUsername { get; set; }
            public string SmtpPassword { get; set; }
            public string FromEmail { get; set; }
        }



        public static async Task SendMailAsync(IConfiguration config, string subject, string body, string to)
        {
            var settings = config.GetSection("EmailSettings").Get<EmailSettings>();

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(settings.FromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(settings.SmtpServer, settings.SmtpPort, SecureSocketOptions.Auto); // or SecureSocketOptions.Auto
            await smtp.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

    public static void SendVerificationEmail(IConfiguration config, string email, string username, string verificationToken)
        {
            var verificationLink = $"http://127.0.0.1:5500?verify={verificationToken}";
            var subject = "Verify your Fichte Chat account";
            var body = $@"Welcome to Fichte Chat, {username}!

            Please verify your email address by clicking the link below:
            {verificationLink}

            This link will expire in 24 hours.

            If you didn't create this account, please ignore this email.

            Thanks,
            The Fichte Chat Team";

            _ = SendMailAsync(config, subject, body, email);
        }
    }
}
