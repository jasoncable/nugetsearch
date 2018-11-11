using System;
namespace NuGetSearch.Import
{
    public static class Extensions
    {
        public static int ToUnixTime(this DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime FromUnixTime(this DateTime dateTime, int seconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(seconds);
        }
    }
}
