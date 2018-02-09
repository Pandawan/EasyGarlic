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
using SharpCompress;
using SharpCompress.Readers;

namespace EasyGarlic {
    public class Network {
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

        public async Task DownloadFiles(string[] url, string[] path, string[] ending)
        {
            using (WebClient wc = new WebClient())
            {
                //List<Task> tasks = new List<Task>();
                for (int i = 0; i < path.Length; i++)
                {
                    string outputPath = path[i].Replace(ending[i], @"\");

                    // tasks.Add(wc.DownloadFileTaskAsync(new Uri(url[i]), path[i]).);
                    await wc.DownloadFileTaskAsync(new Uri(url[i]), path[i]).ContinueWith(async (t) => await UnZipFile(path[i], outputPath));
                }

                //await Task.WhenAll(tasks.ToArray());
            }
        }

        public Task UnZipFile(string from, string to)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            var task = Task.Run(async () =>
            {
                var childTask = Task.Factory.StartNew(() =>
                {
                    using (Stream stream = File.OpenRead(path))
                    using (var reader = ReaderFactory.Open(stream))
                    {
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                            {
                                Console.WriteLine(reader.Entry.Key);
                                reader.WriteEntryToDirectory(path, new ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });

                            }
                        }
                    }
                });

                var awaiter = childTask.GetAwaiter();
                while (!awaiter.IsCompleted)
                {
                    await Task.Delay(50);
                }
            }).ContinueWith((t) =>
            {
                tcs.SetResult(true);
            });

            return tcs.Task;
        }

    }
}
