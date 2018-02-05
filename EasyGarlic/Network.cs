using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic
{
    public class Network
    {
        private string path = @"D:\Projects\VS Projects\EasyGarlic\EasyGarlic\EasyGarlic\easygarlic-api.json";

        public ApiData data;
        private DateTime timeFetched;

        // After 24h, refresh the ApiData
        private double timeCooldown = 24;
        
        public void ConvertToObject(string jsonData)
        {
            // Set the data and record time fetched
            data = JsonConvert.DeserializeObject<ApiData>(jsonData);
        }

        public void FetchData()
        {
            // If diff (in hours) between the two dates is bigger than cooldown
            if ((DateTime.UtcNow - timeFetched).TotalHours > timeCooldown)
            {
                // Update time fetched
                timeFetched = DateTime.UtcNow;
                
                // Read data from json
                data = IO.ReadTo<ApiData>(path);
            }
        }
        
        public async Task DownloadFiles(string[] url, string[] path)
        {
            using (WebClient wc = new WebClient())
            {
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < path.Length; i++) {
                    tasks.Add(wc.DownloadFileTaskAsync(new Uri(url[i]), path[i]));
                }

                await Task.WhenAll(tasks.ToArray());
            }
        }

    }
}
