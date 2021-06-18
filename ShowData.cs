using Microsoft.Azure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Rawcloud.PhotoDataViewer
{
    public class EnergyData
    {
        public string time { get; set; }
        public string power { get; set; }
        public string today_energy { get; set; }
        public string total_energy { get; set; }
    }

    public static class ShowData
    {
        [FunctionName("ShowData")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [Blob("collected/photoData.json", FileAccess.Read)] CloudAppendBlob appendBlob,
            [Blob("collected/template.html", FileAccess.Read)] CloudBlockBlob templateBlob
            )
        {
            log.LogInformation("Function to draw data request.");

            var templateHTML = await templateBlob.DownloadTextAsync();
            var data = await appendBlob.DownloadTextAsync();

            var dataLines = data.Remove(data.Length - 1, 1).Split("\n");
            var lastLine = dataLines[dataLines.Length-1];

            EnergyData lastData = JsonConvert.DeserializeObject<EnergyData>(lastLine);
            var output = templateHTML.Replace("###POWER###", lastData.power).Replace("###TIME###", lastData.time).Replace("###TODAY###", lastData.today_energy).Replace("###TOTAL###", lastData.total_energy);
            return new ContentResult { Content = output, ContentType = "text/html" };
        }
    }
}
