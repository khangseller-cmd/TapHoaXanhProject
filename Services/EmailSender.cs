using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using NguyenDinhMinhKhang_2380600989.Services;  // ← Thêm dòng này

namespace NguyenDinhMinhKhang_2380600989.Services
{
    // Interface - Định nghĩa các method cần có
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendPasswordResetCodeAsync(string email, string code);
    }

    // Class thực tế - Dùng Gmail SMTP
    public class EmailService : IEmailService, IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Method gửi email cơ bản
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Lấy thông tin từ appsettings.json
            var gmailEmail = _configuration["Gmail:Email"];
            var gmailPassword = _configuration["Gmail:Password"];
            var smtpServer = _configuration["Gmail:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Gmail:SmtpPort"] ?? "587");

            // Tạo email
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Tạp Hóa Xanh", gmailEmail));
            mimeMessage.To.Add(new MailboxAddress("", email));
            mimeMessage.Subject = subject;

            // Tạo nội dung HTML
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            // Gửi qua SMTP
            using (var client = new SmtpClient())
            {
                // Kết nối đến Gmail SMTP
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                // Đăng nhập
                await client.AuthenticateAsync(gmailEmail, gmailPassword);

                // Gửi email
                await client.SendAsync(mimeMessage);

                // Ngắt kết nối
                await client.DisconnectAsync(true);
            }
        }

        // Method gửi mã OTP reset password
        public async Task SendPasswordResetCodeAsync(string email, string code)
        {
            var subject = "Mã đặt lại mật khẩu - Tạp Hóa Xanh";

            // HTML template cho email
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd;'>
                    <div style='background: #00b14f; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>🛒 Tạp Hóa Xanh</h1>
                    </div>
                    <div style='padding: 30px; background: #f9f9f9;'>
                        <h2 style='color: #333;'>Đặt lại mật khẩu</h2>
                        <p>Chào bạn,</p>
                        <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                        <p>Mã OTP của bạn là:</p>
                        <div style='background: #00b14f; color: white; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; border-radius: 5px; margin: 20px 0; letter-spacing: 5px;'>
                            {code}
                        </div>
                        <p>Mã này có hiệu lực trong <strong>15 phút</strong>.</p>
                        <p style='color: #666; font-size: 14px;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='color: #999; font-size: 12px;'>
                            Đây là email tự động, vui lòng không trả lời.<br>
                            © 2024 Tạp Hóa Xanh
                        </p>
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }
    }
}