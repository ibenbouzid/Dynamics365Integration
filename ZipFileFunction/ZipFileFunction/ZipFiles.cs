using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;

namespace ZipFileFunction
{
    public static class ZipFiles
    {

            [FunctionName("ZipFiles")]
            public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
            {
                log.Info("C# HTTP trigger function processed a request.");

                // Get request body
                dynamic body = await req.Content.ReadAsAsync<object>();

                var dataStream = new MemoryStream();
                var json = body.files;
                var FileList = JsonConvert.DeserializeObject<List<FilesToZip>>(json.ToString());

                using (var archive = new ZipArchive(dataStream, ZipArchiveMode.Create, true))
                {
                    foreach (var File in FileList)
                    {
                        var entry = archive.CreateEntry(File.Name);
                        using (var entryStream = entry.Open())
                             using (var streamWriter = new StreamWriter(entryStream))
                            {
                              streamWriter.Write(File.Content);
                            }
                    }
                }

                dataStream.Seek(0, SeekOrigin.Begin);
                byte[] zipBytes = dataStream.ToArray();


                return req.CreateResponse(HttpStatusCode.OK, new { ZipContent = zipBytes });
            }

        }

        public class FilesToZip
        {
            public string Name { get; set; }
            public string Content { get; set; }

        }
}
