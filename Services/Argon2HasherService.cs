using System.Security.Cryptography;
using Konscious.Security.Cryptography;
namespace expense_tracker.Services;

public class Argon2HasherService
{
    public byte[] HashPassword(byte[] password, byte[] salt)
    {
        try
        {
            // getting the hash
            var hash = Hash(password, salt);
            return hash;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        
    }

    public bool VerifyHash(byte[] hashedPassword, byte[] inputPassword,  byte[] salt)
    {
        try
        {
            // lets first hash the input passwrod
            var hash = Hash(password: inputPassword, salt: salt);
            return CryptographicOperations.FixedTimeEquals(hash, hashedPassword);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
    }

    private static byte[] Hash(byte[] password, byte[] salt)
    {
        // creating argon2 instance
        var argon2 = new Argon2d(password: password);

        // setting up the props
        argon2.DegreeOfParallelism = 4;
        argon2.MemorySize = 8194;
        argon2.Iterations = 100;
        argon2.Salt = salt;

        // returning the hash
        return argon2.GetBytes(512 / 8);
    }
}