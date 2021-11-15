using System;
using System.Reflection;

namespace CodeTime
{
    class EnvUtil
    {
        public static string SNOWPLOW_FILE = "snowplowEvents.db";
        private static string pluginName = "visualstudio-codetime";
        //
        // sublime = 1, vs code = 2, eclipse = 3, intellij = 4, visualstudio = 6, atom = 7
        //
        private static int pluginId = 6;

        public static int getPluginId()
        {
            return pluginId;
        }

        public static string getPluginName()
        {
            return pluginName;
        }

        public static string GetVersion()
        {
            return string.Format("{0}.{1}.{2}", CodeTimeAssembly.Version.Major, CodeTimeAssembly.Version.Minor, CodeTimeAssembly.Version.Build);
        }

        public static string GetEditorVersioon()
        {
            return Environment.Version.ToString();
        }

        public static string GetOs()
        {
            return Environment.OSVersion.VersionString;
        }

        public static class CodeTimeAssembly
        {
            static readonly Assembly Reference = typeof(CodeTimeAssembly).Assembly;

            public static readonly Version Version = Reference.GetName().Version;
        }

        public static string getHostname()
        {
            return ExecUtil.GetFirstCommandResult("hostname", null);
        }

        public static string getTimezone()
        {
            string timezone;
            if (TimeZone.CurrentTimeZone.DaylightName != null
                && TimeZone.CurrentTimeZone.DaylightName != TimeZone.CurrentTimeZone.StandardName)
            {
                timezone = TimeZone.CurrentTimeZone.DaylightName;
            }
            else
            {
                timezone = TimeZone.CurrentTimeZone.StandardName;
            }
            return timezone;
        }
    }
}
