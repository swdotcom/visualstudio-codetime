using System;

namespace CodeTime
{
    internal class FormatUtil
    {
        public static string HumanizeMinutes(long minutes)
        {
            string str;
            if (minutes == 60)
            {
                str = "1h";
            }
            else if (minutes > 60)
            {
                double hours = Math.Floor((float)minutes / 60);
                double remainder_minutes = (minutes % 60);
                string formatedHrs = String.Format("{0:0}", Math.Floor(hours)) + "h";
                if ((remainder_minutes / 60) % 1 == 0)
                {
                    str = formatedHrs;
                }
                else
                {
                    str = formatedHrs + " " + remainder_minutes + "m";
                }
            }
            else if (minutes == 1)
            {
                str = "1m";
            }
            else
            {
                str = minutes + "m";
            }
            return str;
        }
    }
}
