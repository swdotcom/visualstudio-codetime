﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace CodeTime
{
    public sealed class SessionSummaryManager
    {
        private static Timer timer;
        private static int THIRTY_MINUTES_MILLIS = 1000 * 60 * 30;

        public static void Initialize()
        {
            if (timer == null)
            {
                timer = new Timer(
                      TimerTaskListener,
                      null,
                      1000,
                      THIRTY_MINUTES_MILLIS);
            }
        }

        private static void TimerTaskListener(object stateinfo)
        {
            bool isWinActivated = AppUtil.ApplicationIsActivated();

            if (!isWinActivated)
            {
                DocEventManager.Instance.PostData();
            }

            _ = UpdateSessionSummaryFromServerAsync();

        }

        public static void ÇlearSessionSummaryData()
        {
            SaveSessionSummaryToDisk(new SessionSummary());
        }

        public static void SaveSessionSummaryToDisk(SessionSummary sessionSummary)
        {
            FileManager.WriteSessionSummaryFileContents(sessionSummary);
        }

        public static async Task UpdateSessionSummaryFromServerAsync()
        {
            string api = "/api/v1/user/session_summary";
            HttpResponseMessage response = await HttpManager.AppRequest(HttpMethod.Get, api);
            if (HttpManager.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                SessionSummary summary = JsonConvert.DeserializeObject<SessionSummary>(responseBody);
                _ = UpdateStatusBarWithSummaryDataAsync(summary);
            } else
            {
                _ = UpdateStatusBarWithSummaryDataAsync(null);
            }
        }

        public static async Task UpdateStatusBarWithSummaryDataAsync(SessionSummary summary)
        {
            if (summary == null)
            {
                summary = FileManager.getSessionSummaryFileData();
            }
            long averageDailyMinutesVal = summary.averageDailyMinutes;

            string currentDayMinutesTime = FormatUtil.HumanizeMinutes(summary.currentDayMinutes);

            // Code time today:  4 hrs | Avg: 3 hrs 28 min
            string iconName = summary.currentDayMinutes > averageDailyMinutesVal ? "rocket.png" : "cpaw.png";

            // it's ok not to await on this
            PackageManager.UpdateStatusBarButtonText(currentDayMinutesTime, iconName);
        }

        internal class SessionSummaryResult
        {
            public SessionSummary sessionSummary { get; set; }
            public string status { get; set; }
        }

    }
}
