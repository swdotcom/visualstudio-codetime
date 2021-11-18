using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CodeTime
{
    public sealed class FlowManager
    {

        public static async Task init()
        {
            HttpResponseMessage response = await HttpManager.AppRequest(HttpMethod.Get, "/plugin/flow_sessions");
            if (HttpManager.IsOk(response))
            {
                try
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject sessionInfo = JsonConvert.DeserializeObject<JObject>(responseBody);
                    JArray flowSessions = (JArray)sessionInfo.GetValue("flow_sessions");
                    bool inFlow = (flowSessions.Count > 0);
                    FileManager.UpdateFlowChange(inFlow);
                }
                catch (Exception e)
                {
                    LogManager.Warning("Error retrieving flow sessions: " + e.Message);
                }
            }
        }

        public static async void EnableFlow(bool automated)
        {
            if (!FileManager.IsInFlow())
            {
                JObject jsonObj = new JObject();
                jsonObj.Add("automated", automated);
                await HttpManager.AppRequest(HttpMethod.Post, "/plugin/flow_sessions", jsonObj.ToString());
                FileManager.UpdateFlowChange(true);
            }
            _ = SessionSummaryManager.UpdateStatusBarWithSummaryDataAsync(null);
            _ = PackageManager.RebuildTreeAsync();
        }

        public static async void DisableFlow()
        {
            if (FileManager.IsInFlow())
            {
                await HttpManager.AppRequest(HttpMethod.Delete, "/plugin/flow_sessions");;
                FileManager.UpdateFlowChange(false);
            }
            _ = SessionSummaryManager.UpdateStatusBarWithSummaryDataAsync(null);
            _ = PackageManager.RebuildTreeAsync();
        }

    }
}
