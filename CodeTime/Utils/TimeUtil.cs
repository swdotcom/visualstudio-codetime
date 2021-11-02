using System;
using System.Globalization;

namespace CodeTime
{
    internal class TimeUtil
    {
        public static string GetFormattedDay(long seconds)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return dateTimeOffset.ToString(@"yyyy-MM-dd");
        }

        public static string ToRfc3339String(long seconds)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return dateTimeOffset.ToString(@"yyyy-MM-dd'T'HH:mm:ssZ", DateTimeFormatInfo.InvariantInfo);
        }

        public static bool IsNewDay()
        {
            NowTime nowTime = GetNowTime();
            string currentDay = FileManager.getItemAsString("currentDay");
            return (!nowTime.day.Equals(currentDay)) ? true : false;
        }

        public static NowTime GetNowTime()
        {
            NowTime timeParam = new NowTime();
            timeParam.day = DateTime.Now.ToString(@"yyyy-MM-dd");
            DateTimeOffset offset = DateTimeOffset.Now;
            // utc now in seconds
            timeParam.now = offset.ToUnixTimeSeconds();
            timeParam.now_dt = DateTime.Now;
            // set the offset (will be negative before utc and positive after)
            timeParam.offset_minutes = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
            timeParam.offset_seconds = timeParam.offset_minutes * 60;
            // local now in seconds
            timeParam.local_now = Convert.ToInt64(timeParam.now + timeParam.offset_seconds);
            timeParam.local_day = offset.ToLocalTime().ToString(@"yyyy-MM-dd");


            // start and end of day
            timeParam.start_of_today = StartOfDay();
            timeParam.local_start_of_day = Convert.ToInt64(((DateTimeOffset)timeParam.start_of_today).ToUnixTimeSeconds() + timeParam.offset_seconds);
            timeParam.local_end_of_day = Convert.ToInt64(EndOfDay() + timeParam.offset_seconds);
            timeParam.utc_end_of_day = EndOfDay();

            // yesterday start
            timeParam.start_of_yesterday_dt = StartOfYesterday();
            timeParam.local_start_of_yesterday = Convert.ToInt64(((DateTimeOffset)timeParam.start_of_yesterday_dt).ToUnixTimeSeconds() + timeParam.offset_seconds);

            // week start
            timeParam.start_of_week_dt = StartOfWeek();
            timeParam.local_start_of_week = Convert.ToInt64(((DateTimeOffset)timeParam.start_of_week_dt).ToUnixTimeSeconds() + timeParam.offset_seconds);

            return timeParam;
        }

        public static long GetNowInSeconds()
        {
            long unixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            return unixSeconds;
        }

        public static long EndOfDay()
        {
            DateTime now = DateTime.Now;
            DateTime endOfDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            return ((DateTimeOffset)endOfDay).ToUnixTimeSeconds();
        }

        public static DateTime StartOfDay()
        {
            DateTime now = DateTime.Now;
            DateTime begOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            return begOfToday;
        }

        public static DateTime StartOfYesterday()
        {
            DateTime now = DateTime.Now;
            now = now.AddDays(-1);
            DateTime begOfYesterday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            return begOfYesterday;
        }

        public static DateTime StartOfWeek()
        {
            DateTime now = DateTime.Now;
            DayOfWeek dow = DateTime.Now.DayOfWeek;
            if (dow == DayOfWeek.Sunday)
            {
                // subtract 7
                now = now.AddDays(-7);
            }
            else
            {
                // subtract until it equals sunday
                while (now.DayOfWeek != DayOfWeek.Sunday)
                {
                    now = now.AddDays(-1);
                }
            }
            DateTime begOfWeek = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            // return ((DateTimeOffset)begOfWeek).ToUnixTimeSeconds();
            return begOfWeek;
        }
    }
}
