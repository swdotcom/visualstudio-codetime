using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeTime
{
    class FileManager
    {

        public static string getSoftwareDataDir(bool autoCreate)
        {
            string userHomeDir = Environment.ExpandEnvironmentVariables("%HOMEPATH%");
            string softwareDataDir = userHomeDir + "\\.software";
            if (autoCreate && !Directory.Exists(softwareDataDir))
            {
                try
                {
                    // create it
                    Directory.CreateDirectory(softwareDataDir);
                }
                catch (Exception)
                { }
            }
            return softwareDataDir;
        }

        public static string GetSnowplowStorageFile()
        {
            return getSoftwareDataDir(true) + "\\" + EnvUtil.SNOWPLOW_FILE + ".db";
        }

        public static string getSessionSummaryFile()
        {
            return getSoftwareDataDir(true) + "\\sessionSummary.json";
        }

        private static String getIntegrationsFile()
        {
            return getSoftwareDataDir(true) + "\\integrations.json";
        }

        public static SessionSummary getSessionSummaryFileData()
        {
            try
            {
                string content = File.ReadAllText(getSoftwareDataDir(true) + "\\sessionSummary.json", Encoding.UTF8);
                SessionSummary summary = JsonConvert.DeserializeObject<SessionSummary>(content);
                if (summary == null)
                {
                    return new SessionSummary();
                }
                return summary;
            }
            catch (Exception)
            {
                return new SessionSummary();
            }
        }

        public static JObject getSoftwareSessionFileData()
        {
            try
            {
                string content = File.ReadAllText(getSoftwareSessionFile(), Encoding.UTF8);
                JObject o = JsonConvert.DeserializeObject<JObject>(content);
                if (o == null)
                {
                    return new JObject();
                }
                return o;
            }
            catch (Exception)
            {
                return new JObject();
            }
        }

        public static JObject getDeviceFileData()
        {
            try
            {
                string content = File.ReadAllText(getDeviceFile(), Encoding.UTF8);
                JObject o = JsonConvert.DeserializeObject<JObject>(content);
                if (o == null)
                {
                    return new JObject();
                }
                return o;
            }
            catch (Exception)
            {
                return new JObject();
            }
        }

        public static JObject getFlowChangeData()
        {
            try
            {
                string content = File.ReadAllText(getFlowChangeFile(), Encoding.UTF8);
                JObject o = JsonConvert.DeserializeObject<JObject>(content);
                if (o == null)
                {
                    return new JObject();
                }
                return o;
            }
            catch (Exception)
            {
                return new JObject();
            }
        }

        public static string getSoftwareSessionFile()
        {
            return getSoftwareDataDir(true) + "\\session.json";
        }

        public static string getDeviceFile()
        {
            return getSoftwareDataDir(true) + "\\device.json";
        }

        public static string getFlowChangeFile()
        {
            return getSoftwareDataDir(true) + "\\flowChange.json";
        }

        public static bool SessionSummaryFileExists()
        {
            string file = getSoftwareDataDir(true) + "\\sessionSummary.json";
            return File.Exists(file);
        }

        public static string getLogFile()
        {
            return getSoftwareDataDir(true) + "\\Log.txt";
        }

        public static long getItemAsLong(string key)
        {
            object val = getItem(key);
            if (val != null)
            {
                return Convert.ToInt64(val);
            }
            return 0L;
        }

        public static string getItemAsString(string key)
        {
            object val = getItem(key);
            if (val != null)
            {
                try
                {
                    return val.ToString();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        public static string getItemAsString(string key, string defaultVal)
        {
            object val = getItem(key);
            if (val != null)
            {
                try
                {
                    return val.ToString();
                }
                catch (Exception)
                {
                    return defaultVal;
                }
            }
            return defaultVal;
        }

        public static bool getItemAsBool(string key)
        {
            object val = getItem(key);
            if (val != null)
            {
                try
                {
                    return Convert.ToBoolean(val);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public static object getItem(string key)
        {
            // read the session json file
            JObject o = getSoftwareSessionFileData();
            if (o != null)
            {
                return o.GetValue(key);
            }
            return null;
        }

        public static void setBoolItem(string key, bool val)
        {
            JObject o = getSoftwareSessionFileData();
            o[key] = val;
            WriteFileContents(getSoftwareSessionFile(), o);
        }

        public static void setNumericItem(string key, long val)
        {
            JObject o = getSoftwareSessionFileData();
            o[key] = val;
            WriteFileContents(getSoftwareSessionFile(), o);
        }

        public static void setItem(string key, string val)
        {
            JObject o = getSoftwareSessionFileData();
            o[key] = val;
            WriteFileContents(getSoftwareSessionFile(), o);
        }

        private static JObject GetDeviceJson()
        {
            return getDeviceFileData();
        }

        public static string getPluginUuid()
        {
            string plugin_uuid;
            JObject content = getDeviceFileContent();

            JToken token = content.GetValue("plugin_uuid");
            if (token == null)
            {
                plugin_uuid = Guid.NewGuid().ToString();
                setPluginUuid(plugin_uuid);
            } else
            {
                plugin_uuid = token.ToString();
            }
            return plugin_uuid;
        }

        private static void setPluginUuid(string value)
        {
            writeContentToDeviceFile("plugin_uuid", value);
        }

        public static string getAuthCallbackState(bool autoCreate)
        {
            string auth_callback_state;
            JObject content = getDeviceFileContent();

            JToken token = content.GetValue("auth_callback_state");
            if (token == null)
            {
                auth_callback_state = Guid.NewGuid().ToString();
                setAuthCallbackState(auth_callback_state);
            }
            else
            {
                auth_callback_state = token.ToString();
            }
            return auth_callback_state;
        }

        public static void setAuthCallbackState(string value)
        {
            writeContentToDeviceFile("auth_callback_state", value);
        }

        public static List<IntegrationConnection> GetIntegrations()
        {
            return GetIntegrationsByType(null);
        }

        public static List<IntegrationConnection> GetIntegrationsByType(string type)
        {
            List<IntegrationConnection> integrations = new List<IntegrationConnection>();
            // deserialize JSON directly from a file
            try
            {
                string file = getIntegrationsFile();
                string content = "";
                if (File.Exists(file))
                {

                    content = File.ReadAllText(file, Encoding.UTF8);
                    List<IntegrationConnection> integrationConnections = JsonConvert.DeserializeObject<List<IntegrationConnection>>(content);
                    if (type == null)
                    {
                        return integrationConnections;
                    } else
                    {
                        foreach (IntegrationConnection integrationConnection in integrationConnections)
                        {
                            if (integrationConnection.integration_type != null &&
                                integrationConnection.integration_type.type.Equals(type.ToLower()) &&
                                integrationConnection.status.ToLower().Equals("active"))
                            {
                                integrations.Add(integrationConnection);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error("Error reading integrations file", e);
            }

            return integrations;
        }

        public static void syncIntegrations(List<IntegrationConnection> integrations)
        {
            List<IntegrationConnection> activeConnections = new List<IntegrationConnection> ();
            foreach (IntegrationConnection integrationConnection in integrations)
            {
                if (integrationConnection.status.ToLower().Equals("active"))
                {
                    activeConnections.Add(integrationConnection);
                }
            }
            try
            {
                string content = JsonConvert.SerializeObject(activeConnections);
                File.WriteAllText(getIntegrationsFile(), content, Encoding.UTF8);
            }
            catch (Exception)
            {
                //
            }
            finally
            {

            }
        }

        private static JObject getDeviceFileContent()
        {
            string deviceFile = getDeviceFile();
            if (!File.Exists(deviceFile))
            {
                createDeviceFile();
            }
            return getDeviceFileData();
        }

        private static void createDeviceFile()
        {
            string deviceFile = getDeviceFile();
            if (!File.Exists(deviceFile))
            {
                // create it
                // set the plugin_uuid
                string plugin_uuid = Guid.NewGuid().ToString();
                writeContentToDeviceFile("plugin_uuid", plugin_uuid);
            }
        }

        private static void writeContentToDeviceFile(string key, string value)
        {
            string deviceFile = getDeviceFile();

            JObject o = GetDeviceJson();
            if (o == null)
            {
                o = new JObject();
            }
            o[key] = value;
            WriteFileContents(deviceFile, o);
        }

        public static bool IsInFlow()
        {
            string file = getFlowChangeFile();
            if (!File.Exists(file))
            {
                // create it
                UpdateFlowChange(false);
            }

            JObject o = getFlowChangeData();
            JToken token = o.GetValue("in_flow");
            if (token != null)
            {
                try
                {
                    return Convert.ToBoolean(token);
                } catch (Exception)
                {
                    return false;
                }
                        
            }

            return false;
        }

        public static void UpdateFlowChange(bool val)
        {
            string flowChangeFile = getFlowChangeFile();

            JObject o = getFlowChangeData();
            o["in_flow"] = val;
            WriteFileContents(flowChangeFile, o);
        }

        private static void WriteFileContents(string file, JObject o)
        {
            try
            {
                File.WriteAllText(file, JsonConvert.SerializeObject(o), Encoding.UTF8);
                // using (StreamWriter writer = new StreamWriter(file))
                // {
                    // writer.Write(JsonConvert.SerializeObject(o), Encoding.UTF8);
                // }
            } catch (Exception ex)
            {
                LogManager.Error("Error writing " + file, ex);
            }
        }
    }

}
