namespace leapcert_back.Extensions;

public static class DateTimeExtensions
{
    public static DateTime NowInBrasilia(this DateTime dateTime)
    {
        var brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        return TimeZoneInfo.ConvertTime(DateTime.Now, brasiliaTimeZone);
    }
}