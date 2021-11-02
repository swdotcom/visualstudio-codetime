using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace CodeTime
{
    internal class LaunchUtil
    {
        public static void launchWebDashboard()
        {
            string url = Constants.app_endpoint;
            Process.Start(url);
        }

        public static void launchCodeTimeDashboard()
        {
            Process.Start(Constants.app_endpoint + "/dashboard/code_time?view=summary");
        }

        public static void launchReadme()
        {
            Process.Start("https://www.github.com/swdotcom/swdc-visualstudio");
        }

        public static void launchSettings()
        {
            Process.Start(Constants.app_endpoint + "/preferences");
        }

        public static void launchMailToCody()
        {
            string url = Constants.cody_email_url;
            Process.Start(url);
        }

        public static async void launchLogin(string loginType, bool switching_account)
        {
            try
            {
                string auth_callback_state = FileManager.getAuthCallbackState(true);
                FileManager.setAuthCallbackState(auth_callback_state);
                FileManager.setItem("authType", loginType);

                JObject jsonObj = new JObject();
                jsonObj.Add("plugin", "codetime");
                jsonObj.Add("plugin_uuid", FileManager.getPluginUuid());
                jsonObj.Add("pluginVersion", EnvUtil.GetVersion());
                jsonObj.Add("plugin_id", EnvUtil.getPluginId());
                jsonObj.Add("auth_callback_state", auth_callback_state);

                string jwt = FileManager.getItemAsString("jwt");
                string url = "";
                string element_name = "ct_sign_up_google_btn";
                string icon_name = "google";
                string cta_text = "Sign up with Google";
                if (loginType.Equals("google"))
                {
                    jsonObj.Add("redirect", Constants.app_endpoint);
                    url = Constants.metrics_endpoint + "/auth/google";
                }
                else if (loginType.Equals("github"))
                {
                    jsonObj.Add("redirect", Constants.app_endpoint);
                    element_name = "ct_sign_up_github_btn";
                    icon_name = "github";
                    cta_text = "Sign up with GitHub";
                    url = Constants.metrics_endpoint + "/auth/github";
                }
                else
                {
                    jsonObj.Add("token", jwt);
                    jsonObj.Add("auth", "software");
                    element_name = "ct_sign_up_email_btn";
                    icon_name = "evelope";
                    cta_text = "Sign up with email";
                    url = Constants.app_endpoint + "/email-signup";
                }

                StringBuilder sb = new StringBuilder();
                // create the query string from the json object
                foreach (var x in jsonObj)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(x.Key).Append("=").Append(HttpUtility.UrlEncode(x.Value.ToString(), System.Text.Encoding.UTF8));
                }

                url += "?" + sb.ToString();

                Process.Start(url);

                if (!UserManager.checkingLoginState)
                {
                    FileManager.setBoolItem("switching_account", switching_account);
                    Task.Delay(1000 * 10).ContinueWith((task) => { UserManager.RefetchUserStatusLazily(40); });
                }
            }
            catch (Exception ex)
            {

                LogManager.Error("launchLogin, error : " + ex.Message, ex);
            }

        }

        public static void ConnectSlackWorkspace()
        {
            bool isRegistered = UserManager.CheckRegistration(true);
            if (!isRegistered)
            {
                return;
            }

            JObject jsonObj = new JObject();
            jsonObj.Add("plugin", "codetime");
            jsonObj.Add("plugin_uuid", FileManager.getPluginUuid());
            jsonObj.Add("pluginVersion", EnvUtil.GetVersion());
            jsonObj.Add("plugin_id", EnvUtil.getPluginId());
            jsonObj.Add("auth_callback_state", FileManager.getAuthCallbackState(true));
            jsonObj.Add("integrate", "slack");

            StringBuilder sb = new StringBuilder();
            // create the query string from the json object

            foreach (var kvp in jsonObj)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(kvp.Key).Append("=").Append(HttpUtility.UrlEncode(kvp.Value.ToString(), System.Text.Encoding.UTF8));
            }

            string url = Constants.metrics_endpoint + "/auth/slack?" + sb.ToString();
            Process.Start(url);

            UserManager.ResetCurrentSlackConnectCount();

            Task.Delay(1000 * 12).ContinueWith((task) => { UserManager.RefetchSlackConnectStatusLazily(40); });
        }

        [STAThread]
        public static void ShowNotification(string title, string message)
        {
            Task.Delay(0).ContinueWith((task) =>
            {
                Notification.Show(title, message);
            });
        }
    }
}
