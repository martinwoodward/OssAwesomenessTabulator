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

            // Write to Azure blob
            Console.Out.WriteLine("Opening Azure Blob Store");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString); 
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("output");
            // Make sure output container exists and it is publicly accessible
            container.CreateIfNotExists();
            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // The full monty
            Console.Out.WriteLine("Writing projects_all.json");
            CloudBlockBlob allBlob = container.GetBlockBlobReference("projects_all.json");
            allBlob.Properties.ContentType = "application/json";
            using (Stream blobStream = allBlob.OpenWrite())
            {
                Functions.Write(blobStream, data);
            }
            // Main file
            Console.Out.WriteLine("Writing projects.json");
            CloudBlockBlob activeblob = container.GetBlockBlobReference("projects.json");
            activeblob.Properties.ContentType = "application/json";
            using (Stream blobStream = activeblob.OpenWrite())
            {
                Functions.Write(blobStream, data.Active());
            }
            // Top 50
            Console.Out.WriteLine("Writing projects_top.json");
            CloudBlockBlob topBlob = container.GetBlockBlobReference("projects_top.json");
            topBlob.Properties.ContentType = "application/json";
            using (Stream blobStream = topBlob.OpenWrite())
            {
                Functions.Write(blobStream, data.Top(50));
            }
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

            return Config.LoadFromWeb(url, creds, users);
        }
    }
}
