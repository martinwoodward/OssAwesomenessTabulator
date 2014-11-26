using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using OssAwesomenessTabulator.Data;
using System.Linq;
using Microsoft.WindowsAzure;
using System.Net;
using Octokit;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Collections.Generic;


namespace OssAwesomenessTabulator
{
    class Program
    {
        static void Main()
        {
            System.Diagnostics.Trace.TraceInformation("Starting");
            Console.Out.WriteLine("Starting");
            Execute();
            // JobHost host = new JobHost();
            // host.RunAndBlock();            
            Console.Out.WriteLine("Done");
            System.Diagnostics.Trace.TraceInformation("Done.");
        }

        private static void Execute()
        {
            // Get Data
            OssData data = Functions.GetData(getConfig());

            Console.Out.WriteLine("Found {0} projects in {1} orgs. {2} forks and {3} stars.", 
                data.Summary.Projects, 
                data.Summary.Organizations, 
                data.Summary.Forks, 
                data.Summary.Stars);

            // Write to Azure blob
            Console.Out.WriteLine("Opening Azure Blob Store");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString); 
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Set CORS
            ServiceProperties properties = blobClient.GetServiceProperties();
            properties.Cors = new CorsProperties();
            properties.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedHeaders = new List<string>() { "*" },
                AllowedMethods = CorsHttpMethods.Put | CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Post,
                AllowedOrigins = new List<string>() { "*" },
                ExposedHeaders = new List<string>() { "*" },
                MaxAgeInSeconds = 1800 // Make last 30 minutes in cache
            });
            blobClient.SetServiceProperties(properties);
            Console.Out.WriteLine("CORS Updated");

            CloudBlobContainer container = blobClient.GetContainerReference("output");
            // Make sure output container exists and it is publicly accessible
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // The full monty
            writeBlob(container, data, "projects_all");
            // Main file
            writeBlob(container, data.Active(), "projects");
            // Top 50
            writeBlob(container, data.Top(50), "projects_top");
            // GitHub
            writeBlob(container, data.GitHub().Active(), "gh_projects");
            // GH Top 50
            writeBlob(container, data.GitHub().Top(50), "gh_projects_top");

            Console.Out.WriteLine("Feeds updated.");

        }

        private static void writeBlob(CloudBlobContainer container, OssData data, string filename)
        {
            Console.Out.WriteLine("Writing " + filename);
            // Write JSON version
            CloudBlockBlob jsonBlob = container.GetBlockBlobReference(filename + ".json");
            jsonBlob.Properties.ContentType = "application/json";
            using (Stream blobStream = jsonBlob.OpenWrite())
            {
                Functions.Write(blobStream, data, null);
            }
            Console.Out.WriteLine("  " + jsonBlob.Uri);
            // Write JSONP version
            CloudBlockBlob jsBlob = container.GetBlockBlobReference(filename + ".js");
            jsBlob.Properties.ContentType = "application/javascript";
            using (Stream blobStream = jsBlob.OpenWrite())
            {
                Functions.Write(blobStream, data, "JSON_CALLBACK");
            }
            Console.Out.WriteLine("  " + jsBlob.Uri);
        }
 
        private static Config getConfig()
        {
            Credentials creds = null;
            
            if (ConfigurationManager.ConnectionStrings["GitHubID"] != null)
            {
                creds = new Credentials(ConfigurationManager.ConnectionStrings["GitHubID"].ConnectionString, ConfigurationManager.ConnectionStrings["GitHubPassword"].ConnectionString);
            }
            string url = CloudConfigurationManager.GetSetting("ConfigBaseURL");
            if (String.IsNullOrEmpty(url))
            {
                // Default to Microsoft config.
                url = "https://raw.githubusercontent.com/Microsoft/microsoft.github.io/master/data";
            }
            String[] users = null;
            if (ConfigurationManager.ConnectionStrings["CodePlexUsers"] != null)
            {
                users = ConfigurationManager.ConnectionStrings["CodePlexUsers"].ConnectionString.Split(';');
            }

            Config config = Config.LoadFromWeb(url, creds, users);

            string contributor = CloudConfigurationManager.GetSetting("DefaultContributor");
            if (!String.IsNullOrEmpty(contributor))
            {
                config.DefaultContributor = contributor;
            }

            return config;
        }
    }
}
