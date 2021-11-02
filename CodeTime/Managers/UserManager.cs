using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
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

        public static async Task<bool> IsOnlineAsync()
        {
            long nowInSec = TimeUtil.GetNowInSeconds();
            long thresholdSeconds = nowInSec - lastOnlineCheck;
            if (thresholdSeconds > 60)
            {
                // 3 second timeout
                HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Get, "/ping", null);
                isOnline = HttpManager.IsOk(response);
                lastOnlineCheck = nowInSec;
            }

            return isOnline;
        }

        public static async Task<SoftwareUser> GetUser()
        {
            SoftwareUser user = null;
            HttpResponseMessage response = await HttpManager.MetricsRequest(HttpMethod.Get, "/users/me");

            if (HttpManager.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                user = JsonConvert.DeserializeObject<SoftwareUser>(responseBody);
            }
            return user;
        }
        public static async Task<string> CreateAnonymousUserAsync(bool ignoreJwt)
        {
            // get the app jwt
            try
            {
                string jwt = FileManager.getItemAsString("jwt");
                if (string.IsNullOrEmpty(jwt))
                {
                    string plugin_uuid = FileManager.getPluginUuid();
                    string auth_callback_state = FileManager.getAuthCallbackState(true);
                    string osUsername = Environment.UserName;
                    string timezone = "";
                    if (TimeZone.CurrentTimeZone.DaylightName != null
                        && TimeZone.CurrentTimeZone.DaylightName != TimeZone.CurrentTimeZone.StandardName)
                    {
                        timezone = TimeZone.CurrentTimeZone.DaylightName;
                    }
                    else
                    {
                        timezone = TimeZone.CurrentTimeZone.StandardName;
                    }

                    JObject jsonObj = new JObject();
                    jsonObj.Add("timezone", timezone);
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
                            return jwt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogManager.Error("CreateAnonymousUserAsync, error: " + ex.Message, ex);
            }


            return null;
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

            if (pluginStateInfo == null && !string.IsNullOrEmpty(authType) && (authType.Equals("software") || authType.Equals("email")))
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

                FileManager.setBoolItem("switching_account", false);
                FileManager.setAuthCallbackState(null);
            }

            return user;
        }

        public static async void RefetchUserStatusLazily(int tryCountUntilFoundUser)
        {
            checkingLoginState = true;
            try
            {
                SoftwareUser user = await GetRegistrationState(false);

                if (user == null || user.registered == 0)
                {
                    if (tryCountUntilFoundUser > 0)
                    {
                        tryCountUntilFoundUser -= 1;

                        Task.Delay(1000 * 10).ContinueWith((task) => { RefetchUserStatusLazily(tryCountUntilFoundUser); });
                    }
                    else
                    {
                        // clear the auth, we've tried enough
                        FileManager.setBoolItem("switching_account", false);
                        FileManager.setAuthCallbackState(null);
                        checkingLoginState = false;
                    }
                }
                else
                {
                    checkingLoginState = false;
                    // clear the auth, we've tried enough
                    FileManager.setBoolItem("switching_account", false);
                    FileManager.setAuthCallbackState(null);

                    // clear the time data summary and session summary
                    SessionSummaryManager.Instance.ÇlearSessionSummaryData();

                    // clear the integrations
                    FileManager.syncIntegrations(user.integration_connections);

                    // show they've logged on
                    string msg = "Successfully logged on to Code Time.";
                    LaunchUtil.ShowNotification("Code Time", msg);

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
            SoftwareUser user = await GetUser();
            if (!HasNewIntegration(user))
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

        public static async void DisconnectSlackAuth(string authId)
        {
            IntegrationConnection foundIntegration = FileManager.GetIntegrations().Find(delegate (IntegrationConnection n) { return n.authId.Equals(authId); });
            if (foundIntegration == null)
            {
                LogManager.Warning("Unable to find slack workspace to disconnect");
            }
            string msg = "Are you sure you would like to disconnect the '" + foundIntegration.team_domain + "' Slack workspace?";
            DialogResult res = MessageBox.Show(msg, "Disconnect Slack", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            if (res == DialogResult.Yes)
            {
                string jwt = FileManager.getItemAsString("jwt");
                JObject jsonObj = new JObject();
                jsonObj.Add("authId", foundIntegration.authId);
                HttpManager.MetricsRequest(HttpMethod.Put, "/auth/slack/disconnect", jsonObj.ToString());

                SoftwareUser user = await GetUser();
                FileManager.syncIntegrations(user.integration_connections);

                PackageManager.RebuildTreeAsync();
            }
        }

        public static bool HasNewIntegration(SoftwareUser user)
        {
            if (user != null)
            {
                List<IntegrationConnection> currentIntegrations = FileManager.GetIntegrations();
                foreach (IntegrationConnection integration in user.integration_connections)
                {
                    if (integration.name.ToLower().Equals("slack") && integration.status.ToLower().Equals("active"))
                    {
                        IntegrationConnection foundIntegration = currentIntegrations.Find(delegate (IntegrationConnection n) { return n.authId.Equals(integration.authId); });
                        if (foundIntegration == null)
                        {
                            FileManager.syncIntegrations(currentIntegrations);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
