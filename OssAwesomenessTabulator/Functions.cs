using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OssAwesomenessTabulator
{
    public class Functions
    {

        public static void WriteTop( [Blob("output/{name}_top.json", FileAccess.Write)] Stream output )
        {
            using (System.IO.StreamWriter file = new StreamWriter(output, Encoding.Default))
            {
                file.Write("top");
            }
        }

    }
}
