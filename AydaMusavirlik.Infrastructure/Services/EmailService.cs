using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AydaMusavirlik.Infrastructure.Services;

/// <summary>
/// E-posta gönderme servisi
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailMessage message);
    Task<bool> SendLeaveFormEmailAsync(string toEmail, string toName, LeaveFormPdfModel formData, byte[] pdfAttachment);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(EmailSettings settings)
    {
        _settings = settings;
    }

    public async Task<bool> SendEmailAsync(EmailMessage message)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
            email.Subject = message.Subject;

            var builder = new BodyBuilder();

            if (message.IsHtml)
                builder.HtmlBody = message.Body;
            else
                builder.TextBody = message.Body;

            // Ekler
            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, 
                _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

            if (!string.IsNullOrEmpty(_settings.Username))
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"E-posta gönderme hatasý: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendLeaveFormEmailAsync(string toEmail, string toName, LeaveFormPdfModel formData, byte[] pdfAttachment)
    {
        var htmlBody = GenerateLeaveFormEmailHtml(formData);

        var message = new EmailMessage
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = $"Ýzin Formu - {formData.FormNo}",
            Body = htmlBody,
            IsHtml = true,
            Attachments = new List<EmailAttachment>
            {
                new()
                {
                    FileName = $"IzinFormu_{formData.FormNo}.pdf",
                    Content = pdfAttachment,
                    ContentType = "application/pdf"
                }
            }
        };

        return await SendEmailAsync(message);
    }

    private string GenerateLeaveFormEmailHtml(LeaveFormPdfModel model)
    {
        var statusColor = model.OnayDurumu switch
        {
            "Onaylandý" => "#4CAF50",
            "Reddedildi" => "#F44336",
            _ => "#FF9800"
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #1976D2; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f5f5f5; padding: 20px; }}
        .info-box {{ background: white; padding: 15px; margin: 10px 0; border-radius: 4px; border-left: 4px solid #1976D2; }}
        .status {{ display: inline-block; padding: 8px 16px; background: {statusColor}; color: white; border-radius: 4px; font-weight: bold; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        td {{ padding: 8px; border-bottom: 1px solid #eee; }}
        .label {{ font-weight: bold; width: 40%; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>ÝZÝN TALEBÝ BÝLDÝRÝMÝ</h2>
            <p>Form No: {model.FormNo}</p>
        </div>

        <div class='content'>
            <p>Sayýn <strong>{model.PersonelAdi}</strong>,</p>

            <p>Ýzin talebiniz ile ilgili bilgilendirme aþaðýdadýr:</p>

            <div class='info-box'>
                <table>
                    <tr><td class='label'>Ýzin Türü:</td><td>{model.IzinTuru}</td></tr>
                    <tr><td class='label'>Baþlangýç:</td><td>{model.BaslangicTarihi:dd MMMM yyyy, dddd}</td></tr>
                    <tr><td class='label'>Bitiþ:</td><td>{model.BitisTarihi:dd MMMM yyyy, dddd}</td></tr>
                    <tr><td class='label'>Toplam:</td><td><strong>{model.GunSayisi} gün</strong></td></tr>
                </table>
            </div>

            <p style='text-align: center; margin: 20px 0;'>
                <span class='status'>{model.OnayDurumu.ToUpper()}</span>
            </p>

            {(model.OnayDurumu == "Onaylandý" ? $@"
            <p>Onaylayan: <strong>{model.OnaylayanAdi}</strong><br>
            Onay Tarihi: {model.OnayTarihi:dd.MM.yyyy}</p>

            <p>Ýzin formunuz ekte PDF olarak sunulmuþtur.</p>

            <p>Ýyi tatiller dileriz! ??</p>
            " : "")}

            {(model.OnayDurumu == "Reddedildi" ? $@"
            <p>Red Nedeni: {model.OnayNotu ?? "-"}</p>
            " : "")}
        </div>

        <div class='footer'>
            <p>{model.FirmaAdi}<br>
            Bu e-posta otomatik olarak oluþturulmuþtur.</p>
        </div>
    </div>
</body>
</html>";
    }
}

/// <summary>
/// E-posta mesaj modeli
/// </summary>
public class EmailMessage
{
    public string ToEmail { get; set; } = "";
    public string ToName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsHtml { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}

/// <summary>
/// E-posta ek modeli
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = "";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

/// <summary>
/// E-posta ayarlarý
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = false;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string SenderEmail { get; set; } = "";
    public string SenderName { get; set; } = "Ayda Müþavirlik";
}