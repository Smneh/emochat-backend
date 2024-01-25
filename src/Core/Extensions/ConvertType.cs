namespace Core.Extensions;

public class ConvertType
{
    public static byte[] ConvertStreamToByteArray(Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}