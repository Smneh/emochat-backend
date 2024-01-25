using System.ComponentModel.DataAnnotations;

namespace Core.Utilities;

public static class TypeDescription
{
    public static string GetDescriptionFromEnumName<TEnum>(string name) where TEnum : struct, Enum
    {
        if (!Enum.TryParse(name, out TEnum result))
        {
            throw new ArgumentException($"Invalid enum name: {name}");
        }

        var fi = result.GetType().GetField(result.ToString());
        var attributes = (DisplayAttribute[])fi?.GetCustomAttributes(typeof(DisplayAttribute), false);
        
        return (attributes is {Length: > 0} ? attributes[0].Description : result.ToString())!;
    }
}