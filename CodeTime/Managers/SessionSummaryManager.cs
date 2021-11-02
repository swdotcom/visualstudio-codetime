using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CodeTime
{
    public sealed class SessionSummaryManager
    {
        private static readonly Lazy<SessionSummaryManager> lazy = new Lazy<SessionSummaryManager>(() => new SessionSummaryManager());

        private SessionSummary _sessionSummary;

        public static SessionSummaryManager Instance { get { return lazy.Value; } }

        private SessionSummaryManager()
        {
        }

        public void ÇlearSessionSummaryData()
        {
            _sessionSummary = new SessionSummary();
            SaveSessionSummaryToDisk(_sessionSummary);
        }

        private async Task<SessionSummaryResult> GetSessionSummaryStatusAsync()
        {
            SessionSummaryResult sessionSummaryResult = new SessionSummaryResult();
            _sessionSummary = FileManager.getSessionSummaryFileData();
            sessionSummaryResult.sessionSummary = _sessionSummary;
            sessionSummaryResult.status = "OK";
            return sessionSummaryResult;
        }

        public void SaveSessionSummaryToDisk(SessionSummary sessionSummary)
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

        public async Task UpdateStatusBarWithSummaryDataAsync()
        {
            _sessionSummary = FileManager.getSessionSummaryFileData();
            long averageDailyMinutesVal = _sessionSummary.averageDailyMinutes;

            string currentDayMinutesTime = FormatUtil.HumanizeMinutes(_sessionSummary.currentDayMinutes);

            // Code time today:  4 hrs | Avg: 3 hrs 28 min
            string iconName = _sessionSummary.currentDayMinutes > averageDailyMinutesVal ? "rocket.png" : "cpaw.png";

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
