using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OssAwesomenessTabulator
{
    class Program
    {
        static void Main()
        {
            System.Diagnostics.Trace.TraceInformation("Starting");
            Console.Out.WriteLine("Starting " + DateTime.Now.ToShortTimeString());
            Initialize();
            // JobHost host = new JobHost();
            // host.RunAndBlock();            
            System.Diagnostics.Trace.TraceInformation("Done.");
        }

        private static void Initialize()
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString); 
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("output");
            container.CreateIfNotExists();
            
            CloudBlockBlob blob = container.GetBlockBlobReference("BlobOperations.txt");
            blob.UploadText("Init at " + DateTime.Now.ToShortTimeString()); 
        } 
    }
}
