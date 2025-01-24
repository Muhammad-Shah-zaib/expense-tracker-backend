namespace expense_tracker.Dtos.Graph;

public class WeeklySummaryResponseDto : ResponseDto
{
    public IList<double> CreditData { get; set; } = [];
    public IList<double> DebitData { get; set; } = [];
}
