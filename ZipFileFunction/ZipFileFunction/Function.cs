using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace ZipFileFunction
{
    public static class Function
    {
        [FunctionName("Function2")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            dynamic body = await req.Content.ReadAsAsync<object>();

            var dataStream = new MemoryStream();
            var cloudStorageAccount = new CloudStorageAccount(new StorageCredentials("storage account", "key"), true);
            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(body.container.ToString());
            var blobFileNames = JsonConvert.DeserializeObject <string[]> (body.files.ToString());

            using (var zipOutputStream = new ZipOutputStream(dataStream))
            {
                foreach (var blobFileName in blobFileNames)
                {
                    zipOutputStream.SetLevel(0);
                    var blob = container.GetBlockBlobReference(blobFileName);
                    var entry = new ZipEntry(blobFileName);
                    zipOutputStream.PutNextEntry(entry);
                    blob.DownloadToStream(zipOutputStream);
                }
                zipOutputStream.Finish();
                dataStream.Position = 0;
                zipOutputStream.Close();
            }
            byte[] zipBytes = dataStream.ToArray();
            //string base64String = Convert.ToBase64String(zipBytes);
            string result = System.Text.Encoding.UTF8.GetString(zipBytes);
            //StreamReader reader = new StreamReader(dataStream);
            //string zipBytes = reader.ReadToEnd();


            /*var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = archive.CreateEntry("foo.txt");

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write("Bar!");
                    }
                }

                using (var fileStream = new FileStream(@"C:\Temp\test.zip", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }*/

            return req.CreateResponse(HttpStatusCode.OK, new { ZipFilename = "zipedfile.zip", ZipFile = zipBytes, datastream = dataStream, result = result });
        }
    }
}
