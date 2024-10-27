using System.Security.Cryptography;

namespace expense_tracker.Utilities;

public class RandomSaltGenerator ()
{
    public static string GenerateSalt( int size )
    {
        var salt = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(salt);
    }
}