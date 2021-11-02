using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace CodeTime
{
    public sealed class FlowManager
    {
        private static readonly Lazy<FlowManager> lazy = new Lazy<FlowManager>(() => new FlowManager());
        public static FlowManager Instance { get { return lazy.Value; } }

        private FlowManager()
        {
        }

        public async void init()
        {
            HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Get, "/v1/flow_sessions", null);
            if (HttpManager.IsOk(response))
            {
                try
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray flowSessions = JsonConvert.DeserializeObject<JArray>(responseBody);
                    bool inFlow = (flowSessions != null && flowSessions.Count > 0);
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
                await HttpManager.AppRequest(HttpMethod.Post, "/v1/flow_sessions", jsonObj.ToString());
                FileManager.UpdateFlowChange(true);
            }

            _ = PackageManager.RebuildTreeAsync();
        }

        public static async void DisableFlow()
        {
            if (FileManager.IsInFlow())
            {
                await HttpManager.AppRequest(HttpMethod.Delete, "/v1/flow_sessions", null);
                FileManager.UpdateFlowChange(false);
            }

            _ = PackageManager.RebuildTreeAsync();
        }

    }
}
