namespace expense_tracker.Dtos.Transaction;

public class AddTransactionResponseDto: ResponseDto
{
    public TransactionDto Transaction { get; set; } = new TransactionDto();
}