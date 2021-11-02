using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CodeTime
{
    public sealed class SessionSummaryManager
    {

        public static void ÇlearSessionSummaryData()
        {
            SaveSessionSummaryToDisk(new SessionSummary());
        }

        public static void SaveSessionSummaryToDisk(SessionSummary sessionSummary)
        {
            string sessionSummaryFile = FileManager.getSessionSummaryFile();

            if (FileManager.SessionSummaryFileExists())
            {
                File.SetAttributes(sessionSummaryFile, FileAttributes.Normal);
            }

            try
            {
                File.WriteAllText(sessionSummaryFile, JsonConvert.SerializeObject(sessionSummary), Encoding.UTF8);
            }
            catch (Exception)
            {
                //
            }

        }

        public static async Task UpdateSessionSummaryFromServerAsync()
        {
            string api = "/sessions/summary";
            HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Get, api);
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
