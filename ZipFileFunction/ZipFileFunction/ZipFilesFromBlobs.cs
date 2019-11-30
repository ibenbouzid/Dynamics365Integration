using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.IO.Compression;

namespace ZipFileFunction
{
    public static class ZipFilesFromBlobs
    {
        [FunctionName("ZipFilesFromBlob")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            dynamic body = await req.Content.ReadAsAsync<object>();

            var dataStream = new MemoryStream();
            //var cloudStorageAccount = new CloudStorageAccount(new StorageCredentials(System.Environment.GetEnvironmentVariable("StorageAccount", System.EnvironmentVariableTarget.Process), System.Environment.GetEnvironmentVariable("StorageKey", System.EnvironmentVariableTarget.Process)), true);
            var cloudStorageAccount = new CloudStorageAccount(new StorageCredentials(body.storageName.ToString(), body.StorageKey.ToString()), true);
            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(body.container.ToString());
            var blobFileNames = JsonConvert.DeserializeObject<string[]>(body.files.ToString());

            /*var dataStream = new MemoryStream();
            var blobFileNames = new string[] { "Customer free text invoice.csv","General journal.csv" };
            var cloudStorageAccount = new CloudStorageAccount(new StorageCredentials(System.Environment.GetEnvironmentVariable("StorageAccount", System.EnvironmentVariableTarget.Process), System.Environment.GetEnvironmentVariable("StorageKey", System.EnvironmentVariableTarget.Process)), true);
            var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference("d365processed");*/


            using (var archive = new ZipArchive(dataStream, ZipArchiveMode.Create, true))
            {
                foreach (var blobFileName in blobFileNames)
                {
                    var blob = container.GetBlockBlobReference(blobFileName);
                    var entry = archive.CreateEntry(blobFileName);
                    using (var entryStream = entry.Open())  
                    blob.DownloadToStream(entryStream);
                }
            }

            dataStream.Seek(0, SeekOrigin.Begin);
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(body.packageName.ToString());
            cloudBlockBlob.UploadFromStream(dataStream);



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

            byte[] zipBytes = dataStream.ToArray();
            return req.CreateResponse(HttpStatusCode.OK, new { Zipfilename = body.packageName.ToString(), ZipContent = zipBytes });
        }
    }
}
