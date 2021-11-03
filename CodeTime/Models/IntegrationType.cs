using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeTime
{
    internal class IntegrationType
    {
        public long id { get; set; }
        // i.e. "slack", "spotify", "gcal"
        public string type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image_url { get; set; }

    }
}
