using expense_tracker.Dtos.Transaction;

namespace expense_tracker.Services;

public class TransactionService (ExpenseTrackerContext context)
{
    public async Task<TransactionDto> AddTransactionAsync(AddTransactionRequestDto requestDto)
    {
        try
        {
            var transaction = await context.Transactions.AddAsync(new Transaction()
            {
                Amount = requestDto.Amount,
                Type = requestDto.Type,
                CardNumber = requestDto.CardNumber,
                Date = requestDto.Date,
                Description = requestDto.Description,
                Purpose = requestDto.Purpose,
                UserId = requestDto.UserId,
                Marked = requestDto.Marked
            });

            await context.SaveChangesAsync();

            return new TransactionDto()
                {
                    Id = transaction.Entity.Id,
                    Amount = transaction.Entity.Amount,
                    Type = transaction.Entity.Type,
                    CardNumber = transaction.Entity.CardNumber,
                    Date = transaction.Entity.Date,
                    Description = transaction.Entity.Description ?? "",
                    Purpose = transaction.Entity.Purpose ?? "",
                    UserId = transaction.Entity.UserId,
                    Marked = transaction.Entity.Marked
                };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool ValidateTransactionPurposeAndType(string purpose, string type)
    {
        return this.ValidateTransactionPurpose(purpose) && this.ValidateTransactionType(type);
    }
    private bool ValidateTransactionPurpose(string purpose)
    {
        return Enum.TryParse<PurposeEnum>(purpose, out _);
    }
    private bool ValidateTransactionType(string type)
    {
        return Enum.TryParse<TypeEnum>(type, out _);
    }
}