using System;

namespace CodeTime
{
    public class NowTime
    {
        public long now { get; set; }
        public DateTime now_dt { get; set; }
        public DateTime start_of_today { get; set; }
        public long local_now { get; set; }
        public double offset_minutes { get; set; }
        public double offset_seconds { get; set; }
        public string local_day { get; set; }
        public string day { get; set; }
        public long local_start_of_day { get; set; }
        public long local_end_of_day { get; set; }
        public long utc_end_of_day { get; set; }
        public DateTime start_of_yesterday_dt { get; set; }
        public long local_start_of_yesterday { get; set; }
        public DateTime start_of_week_dt { get; set; }
        public long local_start_of_week { get; set; }

    }
}
