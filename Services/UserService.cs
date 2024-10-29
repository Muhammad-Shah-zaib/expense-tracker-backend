using expense_tracker.Dtos.User;

namespace expense_tracker.Services;

public class UserService (ExpenseTrackerContext context)
{
    public async Task<bool> ValidateUserWithId(int id)
    {
        try
        {
            var user = await context.AppUsers.FirstOrDefaultAsync(au => au.Id == id);
            return user != null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}