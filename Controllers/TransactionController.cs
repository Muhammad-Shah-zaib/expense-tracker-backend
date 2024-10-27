using expense_tracker.Data;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController (ExpenseTrackerContext context)
{
    private readonly ExpenseTrackerContext _context = context;
    
    [HttpGet]
    public IEnumerable<Transaction> Get()
    {
        return _context.Transactions;
    }
}