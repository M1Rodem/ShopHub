namespace ShopHub.Services;

public class SmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger) => _logger = logger;

    public Task SendAsync(string phone, string message)
    {
        // Заглушка. Реальная реализация — SMS-провайдер (Twilio/SMSC).
        _logger.LogInformation("SMS → {Phone} | {Message}", phone, message);
        return Task.CompletedTask;
    }
}