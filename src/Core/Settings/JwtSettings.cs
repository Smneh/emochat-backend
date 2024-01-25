namespace Core.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; }
    public string EncryptionKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int MobileExpirationTimeInDays { get; set; } = default!;
    public int WebExpirationTimeInMinutes { get; set; } = default!;
}