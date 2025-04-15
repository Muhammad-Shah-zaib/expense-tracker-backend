namespace expense_tracker.Dtos.Transaction;

public class GetCreditsSummaryResponseDto : ResponseDto
{
    public decimal CreditsAmount { get; set; }
    public int CreditsCount { get; set; }
}