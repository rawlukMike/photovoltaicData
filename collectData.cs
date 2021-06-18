using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Blob;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace Rawcloud.PhotoDataViewer
{
    public static class collectData
    {
        [FunctionName("collectData")]
        public static async Task Run(
            [TimerTrigger("0 0,10,20,30,40,50 * * * *")]TimerInfo myTimer, ILogger log,
            [Blob("collected/photoData.json", FileAccess.Write)] CloudAppendBlob appendBlob
        )
        {
            var dataDict = new Dictionary<string,string>();
            var labels = new List<String>{"webdata_today_e", "webdata_total_e", "webdata_now_p"};
            TimeZoneInfo warsawZone;
            try 
            {
                warsawZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
            }
            catch 
            {
                warsawZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            }
            DateTime cetTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, warsawZone);
            dataDict.Add("time", cetTime.ToString("dd-MM-yyyy HH:mm"));
            
            var client = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("PhotoPass"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray)); 
            string responseBody = await client.GetStringAsync(Environment.GetEnvironmentVariable("URL"));

            foreach(string line in responseBody.Split("\n"))
            {
                if(line.StartsWith("var "))
                {
                    var x = line.Replace("var","").Replace(" ","").Replace(";","").Replace("\"","");
                    var id = x.Split("=")[0].Trim();
                    var data = x.Split("=")[1].Trim();
                    if(labels.Contains(id))
                    {
                        dataDict.Add(id,data);
                    }
                }
            }

            var dataDictX = new Dictionary<string,string>();
            dataDictX.Add("time", dataDict["time"]);
            foreach(string key in dataDict.Keys)
            {
                if (key=="webdata_today_e")
                {
                    dataDictX.Add("today_energy", dataDict[key]);
                }
                if (key=="webdata_total_e")
                {
                    dataDictX.Add("total_energy", dataDict[key]);
                }
                if (key=="webdata_now_p")
                {
                    dataDictX.Add("power", dataDict[key]);
                }
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            if(!appendBlob.Exists()) appendBlob.UploadText(JsonConvert.SerializeObject(dataDictX)+"\n");
            else appendBlob.AppendText(JsonConvert.SerializeObject(dataDictX)+"\n");
        }
    }
}
