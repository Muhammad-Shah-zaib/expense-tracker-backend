namespace expense_tracker.Dtos.Transaction;

public class TransactionDto
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Description { get; set; } = string.Empty;

    public string CardNumber { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public int UserId { get; set; }
    
    public double Amount { get; set; }

    public bool Marked { get; set; }
}