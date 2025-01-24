using expense_tracker.Dtos.Graph;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/graph")]
public class GraphController : ControllerBase
{
    private readonly ExpensetrackerContext _context;

    public GraphController(ExpensetrackerContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("weekly-summary/{userId:int}")]
    public async Task<IActionResult> GetWeeklyTransactionSummary([FromRoute] int userId)
    {
        try
        {
            // Validate if the user exists
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(new WeeklySummaryResponseDto
                {
                    Success = false,
                    StatusCode = 404,
                    Message = $"User with ID {userId} not found",
                    Errors = new List<string> { "Invalid user ID" },
                    CreditData = new List<double>(),
                    DebitData = new List<double>()
                });
            }

            // Calculate the start date (7 days ago)
            var startDate = DateTime.UtcNow.Date.AddDays(-6);

            // Fetch transactions for the past 7 days grouped by date
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate)
                .GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    CreditSum = g.Where(t => t.Type == "credit").Sum(t => t.Amount),
                    DebitSum = g.Where(t => t.Type == "debit").Sum(t => t.Amount)
                })
                .ToListAsync();

            // Prepare day-wise data
            var creditData = new double[7];
            var debitData = new double[7];
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var transactionForDay = transactions.FirstOrDefault(t => t.Date == date);

                creditData[i] = transactionForDay?.CreditSum ?? 0;
                debitData[i] = transactionForDay?.DebitSum ?? 0;
            }

            // Response
            return Ok(new WeeklySummaryResponseDto
            {
                Success = true,
                StatusCode = 200,
                Message = "Weekly transaction summary fetched successfully",
                Errors = [],
                CreditData = creditData,
                DebitData = debitData
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            // Handle unexpected errors
            return StatusCode(500, new WeeklySummaryResponseDto
            {
                Success = false,
                StatusCode = 500,
                Message = "An error occurred while processing the request",
                Errors = ["Server error..."],
                CreditData = [],
                DebitData = []
            });
        }
    }
}
