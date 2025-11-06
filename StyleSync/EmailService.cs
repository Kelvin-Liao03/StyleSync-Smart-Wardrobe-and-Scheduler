
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StyleSync.Services
{
    public class EmailService
    {
        private const string FromEmail = "kelvinliao03@gmail.com"; // CHANGE
        private const string FromName = "StyleSync Support";
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string EmailPassword = "opht yybt brlp wlxp"; // CHANGE

        public async Task<bool> SendVerificationCode(string toEmail, string username, string code)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromEmail));
                message.To.Add(new MailboxAddress(username, toEmail));
                message.Subject = "StyleSync Password Reset Code";

                message.Body = new TextPart("plain")
                {
                    Text = $"Hi {username},\n\nYour StyleSync password reset code is: {code}\n\nUse this code to reset your password.\n\n- StyleSync Team"
                };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(FromEmail, EmailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Email Error", ex.Message, "OK");
                return false;
            }

        }
    }
}
