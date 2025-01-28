using expense_tracker.Dtos.Graph;
using expense_tracker.Utilities;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/graph")]
public class GraphController(ExpensetrackerContext context) : ControllerBase
{
    private readonly ExpensetrackerContext _context = context;

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
                return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
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
                Errors = new List<string>(),
                CreditData = creditData,
                DebitData = debitData
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ApiResponseHelper.GenerateTransactionNotFoundResponse());
        }
    }

    [HttpGet]
    [Route("{userId:int}")]
    public async Task<IActionResult> GetMonthlyTransactionSummary([FromRoute] int userId)
    {
        try
        {
            // Check if the user exists
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
            }

            // Get the current date and calculate the previous month's start and end
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Fetch transactions for the previous month grouped by week
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
                .GroupBy(t => new
                {
                    WeekNumber = (t.Date.Day - 1) / 7 + 1
                })
                .Select(g => new
                {
                    WeekNumber = g.Key.WeekNumber,
                    CreditSum = Math.Round(g.Where(t => t.Type.ToUpper() == "CREDIT").Sum(t => t.Amount), 2),
                    DebitSum = Math.Round(g.Where(t => t.Type.ToUpper() == "DEBIT").Sum(t => t.Amount), 2)
                })
                .ToListAsync();

            // Initialize weekly data arrays (up to 5 weeks)
            var weeklyCreditData = new double[5];
            var weeklyDebitData = new double[5];

            foreach (var transactionWeek in transactions)
            {
                int weekIndex = transactionWeek.WeekNumber - 1;
                if (weekIndex >= 0 && weekIndex < 5) // Ensure weekIndex is within bounds
                {
                    weeklyCreditData[weekIndex] = transactionWeek.CreditSum;
                    weeklyDebitData[weekIndex] = transactionWeek.DebitSum;
                }
            }

            // Response
            return Ok(new MonthlySummaryResponseDto
            {
                Success = true,
                StatusCode = 200,
                Message = "Monthly transaction summary fetched successfully",
                Errors = [],
                WeeklyCreditData = weeklyCreditData,
                WeeklyDebitData = weeklyDebitData
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Something went wrong");
        }
    }

}
