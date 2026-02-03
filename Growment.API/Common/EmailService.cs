using System.Net;
using System.Net.Mail;

namespace Growment.API.Services.Common
{
    public class EmailService
    {
        private readonly string _fromEmail = "diyanafz9@gmail.com";
        private readonly string _appPassword = "sgdi olzz meia urzi";

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "OTP for Password Change",
                Body = $@"
Your OTP is: {otp}

This OTP is valid for 5 minutes.
Do not share this code with anyone.",
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_fromEmail, _appPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}
