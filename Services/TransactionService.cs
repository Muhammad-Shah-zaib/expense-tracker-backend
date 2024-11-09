using expense_tracker.Dtos.Transaction;
using expense_tracker.Utilities;

namespace expense_tracker.Services;

public class TransactionService (ExpensetrackerContext context)
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
    
    public async Task<TransactionDto> MarkTransactionAsync(int transactionId)
    {
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);
        if (transaction == null) return FakeTupleGenerator.GenerateFakeTransaction();
        
        transaction.Marked = true;
        await context.SaveChangesAsync();
        return new TransactionDto()
        {   
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type,
            CardNumber = transaction.CardNumber,
            Date = transaction.Date,
            Description = transaction.Description ?? "",
            Purpose = transaction.Purpose ?? "",
            UserId = transaction.UserId,
            Marked = transaction.Marked
        };
    }
    public async Task<Transaction?> GetTransactionWithId(int id)
    {
        return await context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
    }
    public bool ValidateTransactionPurposeAndType(string purpose, string type)
    {
        Console.WriteLine($"purpose=> {purpose}, type=> {type}");
        return this.ValidateTransactionPurpose(purpose) && this.ValidateTransactionType(type);
    }
    private bool ValidateTransactionPurpose(string purpose)
    {
        Console.WriteLine(purpose, Enum.TryParse<PurposeEnum>(purpose, out _));
        return Enum.TryParse<PurposeEnum>(purpose, true, out _);
    }
    private bool ValidateTransactionType(string type)
    {
        return Enum.TryParse<TypeEnum>(type, true, out _);
    }
}