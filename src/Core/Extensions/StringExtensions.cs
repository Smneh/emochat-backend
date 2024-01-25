using System.Text.RegularExpressions;

namespace Core.Extensions;

public static class StringExtensions
{
    public static string IndexFormat(this string input)
    {
        // Convert to lowercase
        var lowercaseText = input.ToLower();

        // Remove spaces, non-alphanumeric characters, and Persian characters
        var cleanedText = Regex.Replace(lowercaseText, "[^a-z0-9\\p{IsArabic}_*]+", "");

        return cleanedText;
    }
}