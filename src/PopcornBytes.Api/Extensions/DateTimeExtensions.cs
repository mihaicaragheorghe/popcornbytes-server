namespace PopcornBytes.Api.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTime(this DateTime dt) => (((DateTimeOffset)dt).ToUnixTimeSeconds());

    public static DateTime FromUnixTime(this long unixTime) => DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
}