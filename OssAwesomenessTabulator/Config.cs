using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using OssAwesomenessTabulator.Data;

namespace OssAwesomenessTabulator
{
    public class Config
    {
        private JToken _orgConfig;
        private JToken _projectConfig;

        private Config()
        {
            // Use Config.LoadFromWeb(string url) to construct
        }

        public static Config LoadFromWeb(string url)
        {
            Config config = new Config();

            using (var web = new WebClient())
            {
                String orgUrl = url + "/organization.json";
                String projectUrl = url + "/project.json";
                
                HttpClient client = new HttpClient();
                using (Stream s = client.GetStreamAsync(orgUrl).Result)
                using (StreamReader sr = new StreamReader(s))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    config._orgConfig = JObject.ReadFrom(reader);
                }
                using (Stream s = client.GetStreamAsync(projectUrl).Result)
                using (StreamReader sr = new StreamReader(s))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    config._projectConfig = JObject.ReadFrom(reader);
                }
            }
            return config;
        }

        public IList<Org> GetOrgs()
        {
            return _orgConfig.ToObject<IList<Org>>();
        }

        public IList<Project> GetProjects()
        {
            return _projectConfig.ToObject<IList<Project>>();
        }

    }
}
