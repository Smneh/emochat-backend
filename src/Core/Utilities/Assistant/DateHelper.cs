using System.Globalization;

namespace Core.Utilities.Assistant;

public static class DateHelper
{
    public static int ToDateInt(DateTime date)
    {
        var persianCalendar = new PersianCalendar();
        return Convert.ToInt32(
            $"{persianCalendar.GetYear(date)}{persianCalendar.GetMonth(date).ToString().PadLeft(2, '0')}{persianCalendar.GetDayOfMonth(date).ToString().PadLeft(2, '0')}");
    }

    public static int TodayDateInt()
    {
        var date = DateTime.Now;
        return ToDateInt(date);
    }

    public static int ToTimeInt(DateTime date)
    {
        return Convert.ToInt32($"{date:HH:mm}".Replace(":", ""));
    }
}