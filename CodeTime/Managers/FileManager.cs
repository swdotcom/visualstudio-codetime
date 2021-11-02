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
        public static string getVSReadmeFile()
        {
            return getSoftwareDataDir(true) + "\\VS_README.txt";
        }

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
            string file = getSoftwareDataDir(true) + "\\events.db";
            return file;
        }

        public static bool softwareSessionFileExists()
        {
            string file = getSoftwareDataDir(false) + "\\session.json";
            return File.Exists(file);
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
                return JsonConvert.DeserializeObject<SessionSummary>(content);
            }
            catch (Exception)
            {
                return new SessionSummary();
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

        public static bool LogFileExists()
        {
            string file = getSoftwareDataDir(true) + "\\Log.txt";
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
            string sessionFile = getSoftwareSessionFile();
            if (File.Exists(sessionFile))
            {
                using (StreamReader reader = File.OpenText(sessionFile))
                {
                    JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    if (o != null)
                    {
                        return o.GetValue(key);
                    }
                }
            }
            return null;
        }

        public static void setBoolItem(string key, bool val)
        {
            JObject o = GetSessionJson();
            o[key] = val;

            File.WriteAllText(getSoftwareSessionFile(), JsonConvert.SerializeObject(o), Encoding.UTF8);
        }

        public static void setNumericItem(string key, long val)
        {
            JObject o = GetSessionJson();
            o[key] = val;

            File.WriteAllText(getSoftwareSessionFile(), JsonConvert.SerializeObject(o), Encoding.UTF8);
        }

        public static void setItem(string key, string val)
        {
            JObject o = GetSessionJson();
            o[key] = val;

            File.WriteAllText(getSoftwareSessionFile(), JsonConvert.SerializeObject(o), Encoding.UTF8);
        }

        private static JObject GetSessionJson()
        {
            string sessionFile = getSoftwareSessionFile();
            using (StreamReader reader = File.OpenText(sessionFile))
            {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                if (o == null)
                {
                    o = new JObject();
                }
                return o;
            }
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
                            if (integrationConnection.name.Equals(type.ToLower()))
                            {
                                integrations.Add(integrationConnection);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Warning("Error reading integrations file: " + e.Message);
            }

            return integrations;
        }

        public static void syncIntegrations(List<IntegrationConnection> integrations)
        {
            try
            {
                string content = JsonConvert.SerializeObject(integrations);
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
            using (StreamReader reader = File.OpenText(deviceFile))
            {
                return (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }
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

            JObject o = GetSessionJson();
            o[key] = value;

            File.WriteAllText(deviceFile, JsonConvert.SerializeObject(o), Encoding.UTF8);
        }

        private static JObject GetFlowChangeData()
        {
            using (StreamReader reader = File.OpenText(getFlowChangeFile()))
            {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                if (o == null)
                {
                    return new JObject();
                } else
                {
                    return o;
                }
            }
        }

        public static bool IsInFlow()
        {
            string file = getFlowChangeFile();
            if (!File.Exists(file))
            {
                // create it
                UpdateFlowChange(false);
            }

            JObject o = GetFlowChangeData();
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

            JObject o = GetFlowChangeData();
            o["in_flow"] = val;

            File.WriteAllText(flowChangeFile, JsonConvert.SerializeObject(o), Encoding.UTF8);
        }
    }

}
