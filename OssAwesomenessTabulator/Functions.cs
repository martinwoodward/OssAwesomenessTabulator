using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OssAwesomenessTabulator.Data;
using Octokit;

namespace OssAwesomenessTabulator
{
    public class Functions
    {

        public static OssData GetData (string configurationUrl)
        {
            OssData data = new OssData();

            Config config = Config.LoadFromWeb(configurationUrl);
            IList<Org> orgs = config.GetOrgs();

            foreach (Org org in orgs)
            {
                try
                {
                    data.AddProjects(GitHubUtils.GetGitHubProjects(org).Result.ToArray());
                }
                catch (Exception ex)
                {
                    // Yeah, I know this is bad, but want to try hard for each org
                    // Consider throwing if not one of a set of defined types.
                    System.Diagnostics.Trace.TraceError("Exception detected in org \"{0}\": {1}", org.Name, ex.StackTrace);
                }                
            }

            return (data);
        }

        public static void Write( 
            [Blob("output/{name}.json", FileAccess.Write)] Stream output,
            OssData data)
        {
            using (StreamWriter sw = new StreamWriter(output, Encoding.UTF8))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                JsonSerializer js = JsonSerializer.Create(GetSettings());
                js.Serialize(jw, data);
            }
        }

        public static void WriteTop(
            [Blob("output/{name}_top.json", FileAccess.Write)] Stream output,
            OssData data)
        {
            Write(output, data.Top(50));
        }

        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings {
              NullValueHandling = NullValueHandling.Ignore,
              DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        }

    }
}
