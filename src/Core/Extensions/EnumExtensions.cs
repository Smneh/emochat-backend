using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Core.Extensions;

public static class EnumExtensions
{
    public static IEnumerable<T> GetEnumValues<T>(this T input) where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new NotSupportedException();

        return Enum.GetValues(input.GetType()).Cast<T>();
    }

    public static IEnumerable<T> GetEnumFlags<T>(this T input) where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new NotSupportedException();

        foreach (var value in Enum.GetValues(input.GetType()))
            if ((input as Enum).HasFlag(value as Enum))
                yield return (T)value;
    }

    public static string ToDisplay(this Enum value, DisplayProperty property = DisplayProperty.Name)
    {
        Assert.NotNull(value, nameof(value));

        var attribute = value.GetType().GetField(value.ToString()).GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();

        if (attribute is null)
            return value.ToString();

        var propValue = attribute.GetType().GetProperty(property.ToString()).GetValue(attribute, null);
        return propValue.ToString();
    }

    public static string ToDescription(this Enum value)
    {
        return value.ToDisplay(DisplayProperty.Description);
    }

    public static string GetValue(this Enum value)
    {
        var res = value.ToString();
        return res;
    }

    public static Dictionary<int, string> ToDictionary(this Enum value)
    {
        return Enum.GetValues(value.GetType()).Cast<Enum>().ToDictionary(p => Convert.ToInt32(p), q => q.ToDisplay());
    }
}

public enum DisplayProperty
{
    Description,
    GroupName,
    Name,
    Prompt,
    ShortName,
    Order
}