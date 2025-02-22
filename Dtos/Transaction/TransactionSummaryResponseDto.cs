namespace expense_tracker.Dtos.Transaction;
public class TransactionSummaryResponseDto: ResponseDto
{
    public List<TransactionDto> DayWiseTransactions { get; set; } = [];
}
