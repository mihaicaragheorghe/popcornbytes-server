namespace PopcornBytes.Api.Extensions;

public static class StringExtensions
{
    public static string WithRequiredPrefix(this string str, char prefix)
    {
        if (!str.StartsWith(prefix))
        {
            str = prefix + str;
        }
        return str;
    }
}