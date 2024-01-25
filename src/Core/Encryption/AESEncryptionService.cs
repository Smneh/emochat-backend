

namespace Core.Encryption;

public class AESEncryptionService
{
    public static string DecryptJwt(string encrypted)
    {
        var key = "!#%&(@$^*)!@#$%^";
        return Cipher.DecryptAes(encrypted, key);
    }
}