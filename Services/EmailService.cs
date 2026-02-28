using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using VendingMachineApp.Models;

namespace VendingMachineApp.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage);

    Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlMessage, byte[] fileBytes,
        string fileName);
}

public class EmailService : IEmailService
{
    private readonly VendingMachineContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(VendingMachineContext context, ILogger<EmailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        try
        {
            // Ambil konfigurasi SMTP yang sedang aktif dari database
            var smtpSetting = await _context.SmtpEmails.FirstOrDefaultAsync(s => s.IsActive);
            if (smtpSetting == null)
            {
                _logger.LogError("Tidak ada konfigurasi SMTP yang aktif ditemukan di database.");
                return false;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSetting.SenderEmail, smtpSetting.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);
            using var smtpClient = new SmtpClient(smtpSetting.Host, smtpSetting.Port)
            {
                Credentials = new NetworkCredential(smtpSetting.Username, smtpSetting.Password),
                EnableSsl = smtpSetting.EnableSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Berhasil mengirim email ke {toEmail}.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Gagal mengirim email ke {toEmail}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlMessage,
        byte[] fileBytes, string fileName)
    {
        try
        {
            var smtpSetting = await _context.SmtpEmails.FirstOrDefaultAsync(s => s.IsActive);
            if (smtpSetting == null)
            {
                _logger.LogError("Tidak ada konfigurasi SMTP yang aktif ditemukan di database.");
                return false;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSetting.SenderEmail, smtpSetting.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            if (fileBytes != null && fileBytes.Length > 0)
            {
                var stream = new System.IO.MemoryStream(fileBytes);
                var attachment = new Attachment(stream, fileName, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            using var smtpClient = new SmtpClient(smtpSetting.Host, smtpSetting.Port)
            {
                Credentials = new NetworkCredential(smtpSetting.Username, smtpSetting.Password),
                EnableSsl = smtpSetting.EnableSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Berhasil mengirim email dengan attachment ke {toEmail}.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Gagal mengirim email dengan attachment ke {toEmail}. Error: {ex.Message}");
            return false;
        }
    }
}
