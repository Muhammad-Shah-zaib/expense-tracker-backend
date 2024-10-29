using expense_tracker.Dtos.Transaction;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController (ExpenseTrackerContext context): ControllerBase
{
    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<FetchTransactionResponseDto>> Get([FromRoute] int id)
    {
        var user = await context.AppUsers.FirstOrDefaultAsync(au => au.Id == id);
        if (user == null)
        {
            return NotFound(new FetchTransactionResponseDto()
            {
                StatusCode = 404,
                Message = "User not found",
                Errors = [$"User with id#{id} not found"],
                Transactions = []
            });
        } 
        var transactions = await context.Transactions.Where(t => t.UserId == id)
            .Select(t => new TransactionDto()
            {
                Id = t.Id,
                Amount = t.Amount,
                CardNumber = t.CardNumber,
                Date = t.Date,
                Description = t.Description ?? "",
                Purpose = t.Purpose ?? "",
                UserId = t.UserId,
                Type = t.Type,
                Marked = t.Marked,
            }).ToListAsync();
        
        return Ok(new FetchTransactionResponseDto()
        {
            StatusCode = 200,
            Message = "Success",
            Errors = [],
            Transactions = transactions
        });
    }
}