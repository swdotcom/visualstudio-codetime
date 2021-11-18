namespace CodeTime
{
    internal static class Constants
    {
        internal const string EditorName = "visualstudio";
        internal const long DEFAULT_SESSION_THRESHOLD_SECONDS = 60 * 15;

        internal const string metrics_endpoint = "https://api.software.com";
        internal const string app_endpoint = "https://app.software.com";
        internal const string cody_email_url = "mailto:cody@software.com";

        internal static string EditorVersion
        {
            get
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                if (CodeTimePackage.ObjDte == null)
                {
                    return string.Empty;
                }
                return CodeTimePackage.ObjDte.Version;
            }
        }
    }
}