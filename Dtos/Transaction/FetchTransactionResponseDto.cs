
namespace expense_tracker.Dtos.Transaction;
public class FetchTransactionResponseDto : ResponseDto
{
    // Initialize Transactions as an empty list
    public IList<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}