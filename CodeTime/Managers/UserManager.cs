using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace CodeTime
{
    class UserManager
    {
        public static bool checkingLoginState = false;
        public static bool isOnline = true;
        public static long lastOnlineCheck = 0;
        public static bool clearLoginStateCheck = false;

        public static async Task<SoftwareUser> GetUser()
        {
            SoftwareUser user = null;
            HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Get, "/users/me");

            if (HttpManager.IsOk(response))
            {
                try
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject jsonObj = JsonConvert.DeserializeObject<JObject>(responseBody);

                    user = jsonObj.GetValue("data").ToObject<SoftwareUser>();

                    FileManager.syncIntegrations(user.integration_connections);

                    _ = PackageManager.RebuildTreeAsync();
                }
                catch (Exception ex)
                {
                    LogManager.Error("Error converting user info", ex);
                }
            }
            return user;
        }

        public static void AuthenticationCompleteHandler(SoftwareUser user)
        {
            if (user == null)
            {
                return;
            }

            clearLoginStateCheck = true;
            FileManager.setBoolItem("switching_account", false);
            FileManager.setAuthCallbackState(null);
            if (user.registered == 1)
            {
                FileManager.setItem("jwt", user.plugin_jwt);
                FileManager.setItem("name", user.email);

                _ = AuthenticationSuccessStateReset(user);
            }
        }

        private static async Task AuthenticationSuccessStateReset(SoftwareUser user)
        {
            MessageBox.Show("Successfully logged on to Code Time", "Code Time", MessageBoxButtons.OK, MessageBoxIcon.Information);

            WebsocketManager.Initialize(true);

            FileManager.syncIntegrations(user.integration_connections);

            _ = PackageManager.RebuildTreeAsync();

            _ = SessionSummaryManager.UpdateSessionSummaryFromServerAsync();
        }

        public static async Task InitializeAnonIfNullSessionToken()
        {
            string jwt = FileManager.getItemAsString("jwt");
            if (string.IsNullOrEmpty(jwt))
            {
                string plugin_uuid = FileManager.getPluginUuid();
                string auth_callback_state = FileManager.getAuthCallbackState(true);
                string osUsername = Environment.UserName;

                JObject jsonObj = new JObject();
                jsonObj.Add("timezone", EnvUtil.getTimezone());
                jsonObj.Add("username", osUsername);
                jsonObj.Add("hostname", EnvUtil.getHostname());
                jsonObj.Add("plugin_uuid", plugin_uuid);
                jsonObj.Add("auth_callback_state", auth_callback_state);

                string api = "/plugins/onboard";
                string jsonData = jsonObj.ToString();
                HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Post, api, jsonData);

                if (HttpManager.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    IDictionary<string, object> respObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                    respObj.TryGetValue("jwt", out object jwtObj);
                    jwt = (jwtObj == null) ? null : Convert.ToString(jwtObj);
                    if (jwt != null)
                    {
                        FileManager.setItem("jwt", jwt);
                        FileManager.setBoolItem("switching_account", false);
                        FileManager.setAuthCallbackState(null);
                    }
                }
            }
        }

        private static async Task<PluginStateInfo> GetPluginStateInfoFromResponseAsync(HttpResponseMessage response)
        {

            try
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                PluginStateInfo pluginStateInfo = JsonConvert.DeserializeObject<PluginStateInfo>(responseBody);
                return pluginStateInfo;
            }
            catch (Exception e)
            {
                LogManager.Warning("Error retrieving plugin state info: " + e.Message);
            }
            return null;
        }

        public static async Task<SoftwareUser> GetRegistrationState(bool isIntegration)
        {
            SoftwareUser user = null;
            string jwt = FileManager.getItemAsString("jwt");
            string authType = FileManager.getItemAsString("authType");

            string auth_callback_state = FileManager.getAuthCallbackState(false);

            string api = "/users/plugin/state";

            string token = (!string.IsNullOrEmpty(auth_callback_state)) ? auth_callback_state : jwt;
            HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Get, api, null, token);

            // check to see if we found the user or not
            PluginStateInfo pluginStateInfo = await GetPluginStateInfoFromResponseAsync(response);

            if (pluginStateInfo == null || pluginStateInfo.state.ToLower().Equals("not_found"))
            {
                // use the jwt
                response = await HttpManager.MetricsRequest(HttpMethod.Get, api, null);
                pluginStateInfo = await GetPluginStateInfoFromResponseAsync(response);
            }

            if (pluginStateInfo != null && pluginStateInfo.user != null)
            {
                user = pluginStateInfo.user;
                if (pluginStateInfo.user.registered == 1)
                {
                    // set the name since we found a registered user
                    FileManager.setItem("name", pluginStateInfo.user.email);
                }

                if (string.IsNullOrEmpty(authType))
                {
                    // default to software if auth type is null or empty
                    FileManager.setItem("authType", "software");
                }

                if (!string.IsNullOrEmpty(pluginStateInfo.user.plugin_jwt) && !isIntegration)
                {
                    // set the jwt since its found
                    FileManager.setItem("jwt", pluginStateInfo.user.plugin_jwt);
                }
            }

            return user;
        }

        private static void ResetLoginCheckStates()
        {
            clearLoginStateCheck = false;
            checkingLoginState = false;
            FileManager.setAuthCallbackState(null);
            FileManager.setBoolItem("switching_account", false);
        }

        public static async void RefetchUserStatusLazily(int tryCountUntilFoundUser)
        {
            if (clearLoginStateCheck)
            {
                ResetLoginCheckStates();
                return;
            }

            checkingLoginState = true;
            try
            {
                SoftwareUser user = await GetRegistrationState(false);

                if (user == null || user.registered == 0)
                {
                    if (tryCountUntilFoundUser > 0)
                    {
                        tryCountUntilFoundUser -= 1;

                        Task.Delay(1000 * 12).ContinueWith((task) => { RefetchUserStatusLazily(tryCountUntilFoundUser); });
                    }
                    else
                    {
                        // clear the auth, we've tried enough
                        ResetLoginCheckStates();
                    }
                }
                else
                {
                    ResetLoginCheckStates();

                    _ = AuthenticationSuccessStateReset(user);

                }
            }
            catch (Exception ex)
            {
                LogManager.Error("RefetchUserStatusLazily ,error : " + ex.Message, ex);
                checkingLoginState = false;
            }
        }

        public static bool CheckRegistration(bool showSignup)
        {
            string name = FileManager.getItemAsString("name");
            if (string.IsNullOrEmpty(name))
            {
                // the user is not registerd
                if (showSignup)
                {
                    _ = Task.Delay(0).ContinueWith((task) => { Notification.InitiateSignupFlow(); });
                }
                return false;
            }
            return true;
        }

        private static int slackConnectTryCount = 0;

        public static void ResetCurrentSlackConnectCount()
        {
            slackConnectTryCount = 0;
        }

        public static async void RefetchSlackConnectStatusLazily(int try_count)
        {
            slackConnectTryCount = try_count;
            List<IntegrationConnection> existingConnections = FileManager.GetIntegrations();
            SoftwareUser user = await GetUser();
            if (!HasNewIntegration(user.integration_connections, existingConnections))
            {
                if (slackConnectTryCount > 0)
                {
                    slackConnectTryCount -= 1;
                    Task.Delay(1000 * 10).ContinueWith((task) => { RefetchSlackConnectStatusLazily(slackConnectTryCount); });
                }
                else
                {
                    FileManager.setAuthCallbackState(null);
                    slackConnectTryCount = 0;
                }
            }
            else
            {
                FileManager.setAuthCallbackState(null);
                slackConnectTryCount = 0;

                string msg = "Successfully connected to Slack.";
                LaunchUtil.ShowNotification("Code Time", msg);

                // refresh the tree view
                PackageManager.RebuildTreeAsync();
            }
        }

        public static async void DisconnectSlackAuth(string domain)
        {
            List<IntegrationConnection> connections = FileManager.GetIntegrations();

            IntegrationConnection foundIntegration = null;
            foreach (IntegrationConnection integrationConnection in connections)
            {
                if (integrationConnection.meta != null)
                {
                    IntegrationMeta meta = JsonConvert.DeserializeObject<IntegrationMeta>(integrationConnection.meta);
                    if (meta.domain.Equals(domain))
                    {
                        foundIntegration = integrationConnection;
                        break;
                    }
                }
            }

            if (foundIntegration == null)
            {
                LogManager.Warning("Unable to find slack workspace to disconnect");
            }
            string msg = "Are you sure you would like to disconnect the " + domain + " workspace?";
            DialogResult res = MessageBox.Show(msg, "Disconnect Slack", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            if (res == DialogResult.Yes && foundIntegration != null)
            {
                HttpManager.MetricsRequest(HttpMethod.Delete, "/integrations/" + foundIntegration.id, null);
                _ = GetUser();
            }
        }

        public static bool HasNewIntegration(List<IntegrationConnection> userIntegrations, List<IntegrationConnection> existingIntegrations)
        {
            if (existingIntegrations == null)
            {
                existingIntegrations = new List<IntegrationConnection>();
            }
            if (userIntegrations != null)
            {
                foreach (IntegrationConnection integration in userIntegrations)
                {
                    if (integration.integration_type != null && integration.integration_type.type.ToLower().Equals("slack") && integration.status.ToLower().Equals("active"))
                    {
                        IntegrationConnection foundIntegration = existingIntegrations.Find(delegate (IntegrationConnection n) { return n.authId.Equals(integration.authId); });
                        if (foundIntegration == null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
