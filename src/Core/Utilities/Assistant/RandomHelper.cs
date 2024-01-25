namespace Core.Utilities.Assistant;

public static class RandomHelper
{
    public static int GetRandomIntNumber(int minValue = 10, int maxValue = 20)
    {
        return new Random().Next(minValue, maxValue);
    }
}