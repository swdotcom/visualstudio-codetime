using System;
using System.Collections.Generic;

namespace CodeTime
{
    internal class SoftwareUser
    {
        public long id { get; set; }
        public int registered { get; set; }
        public String email { get; set; }
        public List<IntegrationConnection> integration_connections { get; set; }
        public String plugin_jwt { get; set; }
        
    }
}
