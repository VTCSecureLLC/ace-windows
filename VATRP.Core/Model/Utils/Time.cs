using System;
using System.Diagnostics;

namespace VATRP.Core.Model.Utils
{
    public class Time
    {
        public static readonly DateTime ZeroTime = DateTime.SpecifyKind(DateTime.Parse("1970-01-01 00:00:00"), (DateTimeKind) DateTimeKind.Utc);

        public static long ConvertDateTimeTicksToLong(DateTime dateTime)
        {
            long ticks = 0L;
            try
            {
                TimeSpan span = (TimeSpan) (dateTime - ZeroTime);
                ticks = span.Ticks;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return ticks;
        }

        public static long ConvertDateTimeToLong(DateTime dateTime)
        {
            long totalMilliseconds = 0L;
            try
            {
                TimeSpan span = (TimeSpan) (dateTime - ZeroTime);
                totalMilliseconds = (long) span.TotalMilliseconds;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return totalMilliseconds;
        }

        public static string ConvertDateTimeToString(DateTime dateTime)
        {
            return ((long) ConvertDateTimeToLong(dateTime)).ToString();
        }

        public static string ConvertLocalTimeToUtcTime(DateTime localTime)
        {
            DateTime dateTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);
            return ConvertDateTimeToString(dateTime);
        }

        public static DateTime ConvertLongToDateTime(long timeLong)
        {
            DateTime zeroTime = ZeroTime;
            try
            {
                zeroTime = zeroTime.AddMilliseconds((double) timeLong);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return zeroTime;
        }

        public static DateTime ConvertStringToDateTime(string timeString)
        {
            try
            {
                return ConvertLongToDateTime(Convert.ToInt64(timeString));
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return ZeroTime;
        }

        public static DateTime ConvertUtcTimeToLocalTime(string utcTime)
        {
            if (!string.IsNullOrEmpty( utcTime))
            {
                return DateTime.Now;
            }
            long timeLong = long.Parse(utcTime);
            return TimeZoneInfo.ConvertTime(ConvertLongToDateTime(timeLong), TimeZoneInfo.Local);
        }

        public static string GetTimeTicksUTCString()
        {
            return ((long) ConvertDateTimeTicksToLong(DateTime.UtcNow)).ToString();
        }

        public static long GetTimeUTCInMilliseconds()
        {
            return ConvertDateTimeToLong(DateTime.UtcNow);
        }

        public static long GetTimeUTCInSeconds()
        {
            long totalSeconds = 0L;
            try
            {
                TimeSpan span = (TimeSpan) (DateTime.UtcNow - ZeroTime);
                totalSeconds = (long) span.TotalSeconds;
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return totalSeconds;
        }

        public static string GetTimeUTCString()
        {
            return ConvertDateTimeToString(DateTime.UtcNow);
        }

        public static int GetTimeZoneInSeconds()
        {
            return (int) TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
        }
    }
}

