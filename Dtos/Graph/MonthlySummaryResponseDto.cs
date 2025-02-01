namespace expense_tracker.Dtos.Graph;
public class MonthlySummaryResponseDto: ResponseDto
{
    public IList<double> WeeklyCreditData { get; set; } = [];
    public IList<double> WeeklyDebitData { get; set; } = [];
}
