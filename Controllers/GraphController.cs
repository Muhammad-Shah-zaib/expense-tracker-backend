using System.Globalization;
using expense_tracker.Dtos.Graph;
using expense_tracker.Utilities;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/graph")]
public class GraphController(ExpensetrackerContext context) : ControllerBase
{
    private readonly ExpensetrackerContext _context = context;

    // GET: api/graph/weekly-summary/
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

            // Pakistan Standard Time (UTC+5)
            var timezoneOffset = TimeSpan.FromHours(5);

            // Get current time in local (Karachi) time
            var todayUtc = DateTime.UtcNow;
            var todayLocal = todayUtc + timezoneOffset;
            var daysSinceSunday = (int)todayLocal.DayOfWeek; // Sunday = 0
            var currentWeekStartLocal = todayLocal.Date.AddDays(-daysSinceSunday);

            // Convert local range to UTC for DB query
            var currentWeekStartUtc = currentWeekStartLocal - timezoneOffset;
            var todayUtcDate = todayLocal.Date - timezoneOffset;

            // Fetch transactions within the week
            var transactions = await _context
                .Transactions.Where(t =>
                    t.UserId == userId && t.Date >= currentWeekStartUtc && t.Date <= todayUtcDate
                )
                .ToListAsync();

            // Group by local date (Karachi time)
            var grouped = transactions
                .GroupBy(t => (t.Date + timezoneOffset).Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        CreditSum = g.Where(t => t.Type == "credit").Sum(t => t.Amount),
                        DebitSum = g.Where(t => t.Type == "debit").Sum(t => t.Amount),
                    }
                );

            var daysInCurrentWeek = daysSinceSunday + 1;
            var creditData = new double[daysInCurrentWeek];
            var debitData = new double[daysInCurrentWeek];

            for (int i = 0; i < daysInCurrentWeek; i++)
            {
                var localDate = currentWeekStartLocal.AddDays(i);
                if (grouped.TryGetValue(localDate, out var summary))
                {
                    creditData[i] = summary.CreditSum;
                    debitData[i] = summary.DebitSum;
                }
            }

            return Ok(
                new WeeklySummaryResponseDto
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Weekly transaction summary fetched successfully",
                    Errors = new List<string>(),
                    CreditData = creditData,
                    DebitData = debitData,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ApiResponseHelper.GenerateTransactionNotFoundResponse());
        }
    }

    // GET: api/graph/last-month-credit-debit-summary/5
    [HttpGet]
    [Route("last-month-credit-debit-summary/{userId:int}")]
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
            var startOfMonth = new DateTime(
                today.Year,
                today.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc
            ).AddMonths(-1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Fetch transactions for the previous month grouped by week
            var transactions = await _context
                .Transactions.Where(t =>
                    t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth
                )
                .GroupBy(t => new { WeekNumber = (t.Date.Day - 1) / 7 + 1 })
                .Select(g => new
                {
                    WeekNumber = g.Key.WeekNumber,
                    CreditSum = Math.Round(
                        g.Where(t => t.Type.ToUpper() == "CREDIT").Sum(t => t.Amount),
                        2
                    ),
                    DebitSum = Math.Round(
                        g.Where(t => t.Type.ToUpper() == "DEBIT").Sum(t => t.Amount),
                        2
                    ),
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
            return Ok(
                new MonthlySummaryResponseDto
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Monthly transaction summary fetched successfully",
                    Errors = [],
                    WeeklyCreditData = weeklyCreditData,
                    WeeklyDebitData = weeklyDebitData,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Something went wrong");
        }
    }

    // GET: api/graph/previous-5-month-summary/5
    [HttpGet]
    [Route("previous-5-month-summary/{userId:int}")]
    public async Task<IActionResult> GetPreviousFiveMonthSummary([FromRoute] int userId)
    {
        try
        {
            // Check if the user exists
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
            }

            // Get the last 5 months (excluding current month)
            var today = DateTime.UtcNow;
            var startMonth = new DateTime(
                today.Year,
                today.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc
            ).AddMonths(-5); // Start of 5 months ago
            var endMonth = new DateTime(today.Year, today.Month, 1, 23, 59, 59, DateTimeKind.Utc); // Start of current month

            var transactions = await _context
                .Transactions.Where(t =>
                    t.UserId == userId && t.Date >= startMonth && t.Date < endMonth
                )
                .GroupBy(t => new { Year = t.Date.Year, Month = t.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    CreditSum = Math.Round(
                        g.Where(t => t.Type.ToUpper() == "CREDIT").Sum(t => t.Amount),
                        2
                    ),
                    DebitSum = Math.Round(
                        g.Where(t => t.Type.ToUpper() == "DEBIT").Sum(t => t.Amount),
                        2
                    ),
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            // Initialize response arrays
            double[] creditData = new double[5];
            double[] debitData = new double[5];
            string[] prev5MonthNames = new string[5];

            // Populate response arrays
            for (int i = 0; i < 5; i++)
            {
                var monthDate = startMonth.AddMonths(i);
                prev5MonthNames[i] = monthDate.ToString("MMMM yyyy"); // Example: "September 2024"

                var transactionMonth = transactions.FirstOrDefault(t =>
                    t.Year == monthDate.Year && t.Month == monthDate.Month
                );
                creditData[i] = transactionMonth?.CreditSum ?? 0;
                debitData[i] = transactionMonth?.DebitSum ?? 0;
            }

            // Response
            return Ok(
                new PreviousFiveMonthSummaryResponseDto
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Previous 5-month transaction summary fetched successfully",
                    Errors = [],
                    CreditData = creditData,
                    DebitData = debitData,
                    Prev5MonthNames = prev5MonthNames,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Something went wrong while fetching transaction summary.");
        }
    }

    [HttpGet]
    [Route("category-wise-summary/{userId:int}")]
    public async Task<IActionResult> GetCategoryWiseSummary([FromRoute] int userId)
    {
        try
        {
            // Check if the user exists
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
            }

            // Get the last month's start and end dates
            var today = DateTime.UtcNow;
            var lastMonthStart = new DateTime(
                today.Year,
                today.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc
            ).AddMonths(-1);
            var lastMonthEnd = new DateTime(
                today.Year,
                today.Month,
                1,
                23,
                59,
                59,
                DateTimeKind.Utc
            );

            var transactions = await _context
                .Transactions.Where(t =>
                    t.Type == "credit"
                    && t.UserId == userId
                    && t.Date >= lastMonthStart
                    && t.Date < lastMonthEnd
                )
                .GroupBy(t => t.Purpose)
                .Select(g => new CategorySummaryDto
                {
                    Category = g.Key ?? "",
                    TotalAmount = Math.Round(g.Sum(t => t.Amount), 2),
                })
                .ToListAsync();

            // Response
            return Ok(
                new CategoryWiseSummaryResponseDto
                {
                    Success = true,
                    StatusCode = 200,
                    Message =
                        "Category-wise transaction summary for last month fetched successfully",
                    Errors = [],
                    Data = transactions,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(
                500,
                "Something went wrong while fetching category-wise transaction summary."
            );
        }
    }

    [HttpGet]
    [Route("summary/{userId:int}")]
    public async Task<IActionResult> GetTransactionSummary(
        [FromRoute] int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string interval
    ) // "day", "week", "month"
    {
        // Ensure dates are UTC
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
        try
        {
            // Validate if the user exists
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
            }

            // Fetch transactions within the given date range
            var transactions = await _context
                .Transactions.Where(t =>
                    t.UserId == userId && t.Date >= startDate && t.Date <= endDate
                )
                .ToListAsync();

            if (transactions.Count == 0)
            {
                return Ok(
                    new
                    {
                        Success = true,
                        Message = "No transactions found in the given date range.",
                    }
                );
            }

            List<double> creditData = new List<double>();
            List<double> debitData = new List<double>();
            List<string> yAxisLabels = new List<string>();

            switch (interval.ToLower())
            {
                case "day":
                    var dailyData = transactions
                        .GroupBy(t => t.Date.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            CreditSum = g.Where(t => t.Type == "credit").Sum(t => t.Amount),
                            DebitSum = g.Where(t => t.Type == "debit").Sum(t => t.Amount),
                        })
                        .OrderBy(g => g.Date)
                        .ToList();

                    foreach (var entry in dailyData)
                    {
                        creditData.Add(entry.CreditSum);
                        debitData.Add(entry.DebitSum);
                        yAxisLabels.Add(entry.Date.ToString("yyyy-MM-dd"));
                    }
                    break;

                case "week":
                    var weeklyData = transactions
                        .GroupBy(t =>
                            CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                t.Date,
                                CalendarWeekRule.FirstFourDayWeek,
                                DayOfWeek.Monday
                            )
                        )
                        .Select(g => new
                        {
                            WeekNumber = g.Key,
                            CreditSum = g.Where(t => t.Type == "credit").Sum(t => t.Amount),
                            DebitSum = g.Where(t => t.Type == "debit").Sum(t => t.Amount),
                        })
                        .OrderBy(g => g.WeekNumber)
                        .ToList();

                    foreach (var entry in weeklyData)
                    {
                        creditData.Add(entry.CreditSum);
                        debitData.Add(entry.DebitSum);
                        yAxisLabels.Add($"Week {entry.WeekNumber}");
                    }
                    break;

                case "month":
                    var monthlyData = transactions
                        .GroupBy(t => new { t.Date.Year, t.Date.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            CreditSum = g.Where(t => t.Type == "credit").Sum(t => t.Amount),
                            DebitSum = g.Where(t => t.Type == "debit").Sum(t => t.Amount),
                        })
                        .OrderBy(g => g.Year)
                        .ThenBy(g => g.Month)
                        .ToList();

                    foreach (var entry in monthlyData)
                    {
                        creditData.Add(entry.CreditSum);
                        debitData.Add(entry.DebitSum);
                        yAxisLabels.Add(
                            new DateTime(entry.Year, entry.Month, 1).ToString("MMMM yyyy")
                        );
                    }
                    break;

                default:
                    return BadRequest(
                        new
                        {
                            Success = false,
                            Message = "Invalid interval. Use 'day', 'week', or 'month'.",
                        }
                    );
            }

            return Ok(
                new
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Transaction summary fetched successfully",
                    CreditData = creditData,
                    DebitData = debitData,
                    YAxisLabels = yAxisLabels,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Something went wrong while fetching transaction summary.");
        }
    }
}
