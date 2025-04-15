using expense_tracker.Dtos.Transaction;
using expense_tracker.Utilities;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController(ExpensetrackerContext context, TransactionService transactionService, UserService userService) : ControllerBase
{

    // GET api/transaction?userId={userId}
    [HttpGet]
    public async Task<ActionResult<FetchTransactionResponseDto>> Get([FromQuery] int userId)
    {
        var isValidUser = await userService.ValidateUserWithId(userId);
        if (!isValidUser)
        {
            return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
        }

        var transactions = await context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Id)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                CardNumber = t.CardNumber,
                Date = t.Date, // Format the Date here
                Description = t.Description ?? "",
                Purpose = t.Purpose ?? "",
                UserId = t.UserId,
                Type = t.Type,
                Marked = t.Marked,
            })
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        return Ok(new FetchTransactionResponseDto
        {
            StatusCode = 200,
            Message = "Success",
            Errors = new List<string>(),
            Transactions = transactions
        });
    }

    // POST api/transaction/id
    [HttpPost]
    public async Task<ActionResult<AddTransactionResponseDto>> Post(
        [FromBody] AddTransactionRequestDto requestDto)
    {
        // validating the user
        var user = await context.AppUsers.FirstOrDefaultAsync(au => au.Id == requestDto.UserId);
        if (user == null) return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(requestDto.UserId));

        // validating type & purpose fields
        // var result = transactionService.ValidateTransactionPurposeAndType(purpose:requestDto.Purpose, type:requestDto.Type);
        // if (!result)
        // {
        //     return BadRequest(ApiResponseHelper.GenerateTransactionPurposeOrTypeErrorResponse());
        // }

        // adding transaction
        var transaction = await transactionService.AddTransactionAsync(requestDto);

        return Ok(new AddTransactionResponseDto()
        {
            StatusCode = 200,
            Message = "success",
            Errors = [],
            Transaction = transaction
        });
    }

    // Put api/transaction/{id}
    [HttpPut]
    [Route("{id:int}")]
    public async Task<ActionResult<UpdateTransactionResponseDto>> Put([FromRoute] int id, UpdateTransactionRequestDto transactionDto)
    {
        // validating transaction
        var transaction = await transactionService.GetTransactionWithId(id);
        if (transaction == null)
        {
            return NotFound(ApiResponseHelper.GenerateTransactionNotFoundResponse());
        }

        // validating purpose types
        if (!transactionService.ValidateTransactionPurposeAndType(purpose: transactionDto.Purpose,
            type: transactionDto.Type)) return BadRequest(ApiResponseHelper.GenerateTransactionPurposeOrTypeErrorResponse());

        // updating transaction
        transaction.Amount = transactionDto.Amount;
        transaction.CardNumber = transactionDto.CardNumber;
        transaction.Description = transactionDto.Description;
        transaction.Purpose = transactionDto.Purpose;
        transaction.Type = transactionDto.Type;
        transaction.Marked = transactionDto.Marked;

        await context.SaveChangesAsync();
        return Ok(new UpdateTransactionResponseDto()
        {
            StatusCode = 200,
            Success = true,
            Message = "updated succedssfully",
            Errors = [],
            Transaction = new TransactionDto()
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                CardNumber = transaction.CardNumber,
                Date = transaction.Date,
                Description = transaction.Description ?? "",
                Purpose = transaction.Purpose ?? "",
                UserId = transaction.UserId,
                Type = transaction.Type,
                Marked = transaction.Marked
            }
        });
    }

    // patch api/transaction/{id}/mark
    [HttpPatch]
    [Route("{id:int}/mark")]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromQuery] int userId)
    {
        var user = await userService.GetUserById(userId);
        if (user == null) return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));

        // getting and validating transaction
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => (t.Id == id && t.UserId == userId));
        if (transaction == null) return NotFound(ApiResponseHelper.GenerateTransactionNotFoundResponse());

        // updating transaction
        transaction.Marked = true;

        await context.SaveChangesAsync();

        return Ok(ApiResponseHelper.GenerateTransactionMarkedSuccessResponse());
    }

    [HttpGet]
    [Route("summary/{userId:int}")]
    public async Task<IActionResult> GetTransactionSummary(
       [FromRoute] int userId,
       [FromQuery] DateTime startDate,
       [FromQuery] DateTime endDate)
    {
        // Ensure dates are in UTC
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc).AddDays(-1);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        try
        {
            // Validate if the user exists
            var userExists = await context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(new { Success = false, StatusCode = 404, Message = $"User with ID {userId} not found." });
            }

            // Fetch transactions in the given date range
            var transactions = await context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            if (transactions.Count == 0)
            {
                return Ok(new TransactionSummaryResponseDto());
            }

            // Group and map transactions
            var dayWiseTransactions = transactions
                .GroupBy(t => t.Date.Date)
                .SelectMany(g => g.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Date = t.Date,
                    Description = t.Description ?? string.Empty,
                    CardNumber = t.CardNumber,
                    Purpose = t.Purpose ?? string.Empty,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    Marked = t.Marked
                })).ToList();

            var response = new TransactionSummaryResponseDto
            {
                DayWiseTransactions = dayWiseTransactions,
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { Success = false, StatusCode = 500, Message = "An internal server error occurred while fetching the transaction summary." });
        }
    }

    [HttpGet]
    [Route("summary-credits/{userId:int}")]
    public async Task<ActionResult<GetCreditsSummaryResponseDto>> GetCreditsSummary(
    [FromRoute] int userId,
    [FromQuery] string creditReportType)
    {
        try
        {
            // Validate if the user exists
            var userExists = await context.AppUsers.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(new
                {
                    Success = false,
                    StatusCode = 404,
                    Message = $"User with ID {userId} not found."
                });
            }

            // Determine date range based on creditReportType
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.UtcNow;

            if (creditReportType?.ToLower() == "this-month")
            {
                startDate = new DateTime(endDate.Year, endDate.Month, 1).AddDays(-1);
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            }

            // Fetch credits based on the determined date range
            var creditsQuery = context.Transactions.Where(c => c.UserId == userId && c.Type == "credit");

            if (creditReportType?.ToLower() == "this-month")
            {
                creditsQuery = creditsQuery.Where(c => c.Date >= startDate && c.Date <= endDate);
            }

            var credits = await creditsQuery.ToListAsync();

            if (credits.Count == 0)
            {
                return Ok(new GetCreditsSummaryResponseDto
                {
                    CreditsAmount = 0,
                    CreditsCount = 0,
                    Success = true,
                    StatusCode = 200,
                    Message = creditReportType?.ToLower() == "this-month"
                        ? "No credits found for this month."
                        : "No credits found."
                });
            }

            // Calculate total credits amount
            int totalCreditsAmount = (int)credits.Sum(c => c.Amount);

            // Prepare the response
            var response = new
            {
                creditsAmount = totalCreditsAmount,
                creditsCount = credits.Count,
                Success = true,
                StatusCode = 200,
                Message = "Credits summary fetched successfully."
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new
            {
                Success = false,
                StatusCode = 500,
                Message = "An internal server error occurred while fetching the credits summary."
            });
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] int userId)
    {
        var user = await userService.GetUserById(userId);
        if (user == null) return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));

        var transaction = await context.Transactions.FirstOrDefaultAsync(t => (t.Id == id && t.UserId == userId));
        if (transaction == null) return NotFound(ApiResponseHelper.GenerateTransactionNotFoundResponse());

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();

        return Ok(ApiResponseHelper.GenerateTransactionDeletedSuccessResponse(id));
    }

}

