using expense_tracker.Dtos.Transaction;
using expense_tracker.Utilities;
using Microsoft.AspNetCore.Server.HttpSys;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController (ExpenseTrackerContext context, TransactionService transactionService, UserService userService): ControllerBase
{
    
    // GET api/transaction
    [HttpGet]
    public async Task<ActionResult<FetchTransactionResponseDto>> Get([FromBody] int userId)
    {
        var result = await userService.ValidateUserWithId(userId);
        if (!result)
        {
            return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(userId));
        } 
        var transactions = await context.Transactions.Where(t => t.UserId == userId)
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
    
    // POST api/transaction/id
    [HttpPost]
    public async Task<ActionResult<AddTransactionResponseDto>> Post(
        [FromBody] AddTransactionRequestDto requestDto)
    {
        // validating the user
        var user = await context.AppUsers.FirstOrDefaultAsync(au => au.Id == requestDto.UserId);
        if (user == null) return NotFound(ApiResponseHelper.GenerateUserNotFoundResponse(requestDto.UserId));
        
        // validating type & purpose fields
        var result = transactionService.ValidateTransactionPurposeAndType(purpose:requestDto.Purpose, type:requestDto.Type);
        if (!result)
        {
            return BadRequest(ApiResponseHelper.GenerateTransactionPurposeOrTypeErrorResponse());
        }
        
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
        if(!transactionService.ValidateTransactionPurposeAndType(purpose: transactionDto.Purpose,
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
    
}