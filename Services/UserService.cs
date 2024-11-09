namespace expense_tracker.Services;

public class UserService (ExpensetrackerContext context)
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

    public async Task<AppUser?> GetUserById(int id)
    {
        try
        {
            return await context.AppUsers.FirstOrDefaultAsync(au => au.Id == id);
        }catch(Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}