using expense_tracker.Dtos.User;

namespace expense_tracker.Utilities;

public abstract class FakeTupleGenerator
{
    public static UserDto GenerateFakeUser()
    {
        return new UserDto()
        {
            Username = string.Empty,
        };
    }
}