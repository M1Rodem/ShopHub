namespace ShopHub.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body)
    {
        // Заглушка. Реальная реализация — SMTP / SendGrid / Mailgun.
        _logger.LogInformation("EMAIL → {To} | {Subject}", to, subject);
        return Task.CompletedTask;
    }
}