using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class InstallManager {

        public class ToInstall {
            public ApiData.MinerData data;
            public string id;
            public string platform;
            public string path;

            public ToInstall(ApiData.MinerData _data, string _id, string _platform, string _path)
            {
                data = _data;
                id = _id;
                platform = _platform;
                path = _path;
            }
        }

        public LocalData data;
        public Network net;


        public async Task Setup()
        {
            data = new LocalData();
            // Load the LocalData object from data.json file
            data.Load();

            net = new Network();
            // Load API data
            net.FetchData();

            // If versions do not match
            if (VersionCheck(data, net) == false)
            {
                // TODO: Start process to download new update
            }

            // Check for Miners Updates and download if so
            await UpdateMiners();
        }

        public bool VersionCheck(LocalData d, Network n)
        {
            return (d.version != n.data.app_data.version);
        }

        public async Task UpdateMiners()
        {
            // If some miners have updates
            ToInstall[] toUpdate = MinerCheck(data, net);
            if (toUpdate.Length > 0)
            {
                // Install those updates
                await InstallMiners(toUpdate);
            }

            // Save
            data.Save();
        }

        public ToInstall[] MinerCheck(LocalData d, Network n)
        {
            List<ToInstall> toUpdate = new List<ToInstall>();

            // Get a list of every miner who's version is different from the API version
            foreach (var i in d.installed)
            {
                if (i.Value.version != n.data.miners[i.Value.id].version)
                {
                    toUpdate.Add(new ToInstall(n.data.miners[i.Value.id], i.Value.id, i.Value.platform, i.Value.path));
                }
            }

            return toUpdate.ToArray<ToInstall>();
        }

        public async Task InstallMiners(ToInstall[] install)
        {
            string[] paths = new string[install.Length];
            string[] urls = new string[install.Length];

            // Create an array of every URL to download and every path so save to
            for (int i = 0; i < install.Length; i++)
            {
                paths[i] = install[i].path;
                urls[i] = install[i].data.GetURLFromPlatform(install[i].platform);
            }

            // Batch download all the files
            await net.DownloadFiles(urls, paths);

            // Register them as downloaded/updated
            for (int i = 0; i < install.Length; i++)
            {
                data.installed[install[i].id] = new LocalData.Miner()
                {
                    id = install[i].id,
                    path = paths[i],
                    platform = install[i].platform,
                    version = install[i].data.version
                };
            }
        }

    }
}
