using ShopHub.Data;
using ShopHub.Models;

namespace ShopHub.Services;

public class PaymentService
{
    private readonly ShopHubDbContext _db;

    public PaymentService(ShopHubDbContext db) => _db = db;

    public async Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, string provider)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            Provider = provider,
            Status = "Pending"
        };

        try
        {
            // «десь был бы вызов внешнего API провайдера.
            // ƒл€ демо Ч эмул€ци€ успешной оплаты.
            payment.ExternalId = Guid.NewGuid().ToString("N");
            payment.Status = "Completed";

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment;
        }
        catch
        {
            payment.Status = "Failed";
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            throw;
        }
    }

    public async Task RefundAsync(int paymentId)
    {
        var payment = await _db.Payments.FindAsync(paymentId)
            ?? throw new InvalidOperationException("ѕлатЄж не найден");

        payment.Status = "Refunded";
        await _db.SaveChangesAsync();
    }
}