
namespace CodeTime
{
    class PluginStateInfo
    {
        public string state { get; set; } // "OK"
        public string jwt { get; set; }
        public string email { get; set; }
        public SoftwareUser user { get; set; }
    }
}
