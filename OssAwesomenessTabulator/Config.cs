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
        private string _configdata;
        private JToken _config;

        private Config()
        {
            // Use Config.LoadFromWeb(string url) to construct
        }

        public static Config LoadFromWeb(string url)
        {
            Config config = new Config();

            using (var web = new WebClient())
            {
                config._configdata = web.DownloadString(url);
                
                HttpClient client = new HttpClient();
                using (Stream s = client.GetStreamAsync(url).Result)
                using (StreamReader sr = new StreamReader(s))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    config._config = JObject.ReadFrom(reader);
                }
            }
            return config;
        }

        public IList<Org> GetOrgs()
        {
            return _config.ToObject<IList<Org>>();
        }

    }
}
