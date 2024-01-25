using System.Security.Cryptography;

namespace Services.Services;

public class SecurityService
{
    private const int SaltSize = 32; // You can adjust the salt size based on your requirements
    private const int Iterations = 10000; // You can adjust the number of iterations based on your security needs
    
    public string GenerateHash(string password)
    {
        // Generate a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

        // Create a new instance of Rfc2898DeriveBytes and hash the password with the salt
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        var hash = pbkdf2.GetBytes(32); // 32 is the size of the resulting hash

        // Combine the salt and hash into a single byte array
        var hashBytes = new byte[SaltSize + 32];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, 32);

        // Convert the byte array to a base64-encoded string
        var base64Hash = Convert.ToBase64String(hashBytes);

        return base64Hash;
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        // Convert the base64-encoded string back to a byte array
        var hashBytes = Convert.FromBase64String(hashedPassword);

        // Extract the salt from the byte array
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Create a new instance of Rfc2898DeriveBytes and hash the entered password with the extracted salt
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        var hash = pbkdf2.GetBytes(32); // 32 is the size of the resulting hash

        // Compare the computed hash with the stored hash
        for (var i = 0; i < 32; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
            {
                return false; // Passwords don't match
            }
        }

        return true; // Passwords match
    }
}