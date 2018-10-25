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
            var cloudStorageAccount = new CloudStorageAccount(new StorageCredentials("ukieb", "nRabLGIiS5b3O8XPf8F0AI91uzNN4zxzDJePhDu1tXf6hH3iGYX1M1gR2AEhp1p9jUo8Tmf+ik2SjAHc7E89fw=="), true);
            //var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference("assetphotos");
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
            string base64String = Convert.ToBase64String(zipBytes);

            return req.CreateResponse(HttpStatusCode.OK, new { ZipFilename = "zipedfile.zip", ZipFile = zipBytes });
        }
    }
}
