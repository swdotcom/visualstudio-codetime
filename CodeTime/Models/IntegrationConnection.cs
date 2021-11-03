using System.Collections.Generic;

namespace CodeTime
{
    internal class IntegrationConnection
    {
        public long id { get; set; }
        public long user_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string value { get; set; }
        public string status { get; set; }
        public string authId { get; set; }
        public string auth_id { get; set; }
        public string meta { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string plugin_uuid { get; set; }
        public string team_domain { get; set; }
        public string team_name { get; set; }
        public long integration_type_id { get; set; }
        public IntegrationType integration_type { get; set; }

    }
}
