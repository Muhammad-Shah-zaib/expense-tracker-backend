namespace expense_tracker.Dtos.Graph
{
     public class PreviousFiveMonthSummaryResponseDto: ResponseDto
    {
        public double[] CreditData { get; set; } = new double[5];
        public double[] DebitData { get; set; } = new double[5];
        public string[] Prev5MonthNames { get; set; } = new string[5];
    }
}