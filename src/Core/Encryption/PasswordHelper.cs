using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Encryption;

public static class PasswordHelper
{
    public static string EncryptString(string strToEncrypt)
    {
        var r = new RSACryptoServiceProvider();

        var ue = new UTF8Encoding();
        var bytes = ue.GetBytes(strToEncrypt);
        var s = r.Encrypt(bytes, true);
#pragma warning disable SYSLIB0021
        var md5 = new MD5CryptoServiceProvider();
#pragma warning restore SYSLIB0021
        var hashBytes = md5.ComputeHash(bytes);
        // Bytes to string
        return Regex.Replace(BitConverter.ToString(hashBytes), "-", "").ToLower();
    }
}