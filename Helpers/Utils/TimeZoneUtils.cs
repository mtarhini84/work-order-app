namespace WorkOrderApp.Helpers.Utils
{
    public static class TimeZoneUtils
    {
        public static DateTime LocalTimeToUtc(DateTime localDateTime, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
        }

        public static DateTime UtcToLocalTime(DateTime utcDateTime, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
        }

        public static DateTime? UtcToLocalTime(DateTime? utcDateTime, string timeZoneId)
        {
            if (!utcDateTime.HasValue) return null;
            return UtcToLocalTime(utcDateTime.Value, timeZoneId);
        }

        public static DateTime RoundUpToEndOfDay(DateTime date)
            => new(date.Year, date.Month, date.Day, 23, 59, 59);

        public static (DateTime Start, DateTime End) NormalizeRange(DateTime start, DateTime end, string timeZoneId)
            => (LocalTimeToUtc(start, timeZoneId), LocalTimeToUtc(RoundUpToEndOfDay(end), timeZoneId));
    }
}
