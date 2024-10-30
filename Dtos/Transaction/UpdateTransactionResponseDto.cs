namespace expense_tracker.Dtos.Transaction;

public class UpdateTransactionResponseDto: ResponseDto
{
    public TransactionDto Transaction { get; set; } = new TransactionDto();
}