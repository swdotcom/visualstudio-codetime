using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CodeTime
{
    public class CodeTimeProject
    {
        public string name;
        public string directory;
        public string identifier;

        public CodeTimeProject(string nameVal, string directoryVal)
        {
            name = nameVal;
            directory = directoryVal;
            // get the identifier
            RepoResourceInfo resourceInfo = GitUtilManager.GetResourceInfo(directoryVal);
            if (resourceInfo != null && resourceInfo.identifier != null)
            {
                identifier = resourceInfo.identifier;
            }
        }

        public IDictionary<string, object> GetAsDictionary()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("name", this.name);
            dict.Add("directory", this.directory);
            dict.Add("identifier", this.identifier);
            return dict;
        }

        public JObject GetAsJson()
        {
            JObject jsonObj = new JObject();
            jsonObj.Add("name", this.name);
            jsonObj.Add("directory", this.directory);
            jsonObj.Add("identifier", this.identifier);
            return jsonObj;
        }
    }
}
