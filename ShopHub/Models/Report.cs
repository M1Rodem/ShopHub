namespace ShopHub.Models;

// Не сущность БД, а DTO для отчётов
public class Report
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrdersCount { get; set; }
    public int ProductsSold { get; set; }
    public List<TopProductRow> TopProducts { get; set; } = new();
}

public class TopProductRow
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}