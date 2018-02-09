using System;
using System.Collections.Generic;
using System.IO;
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
        
        public async Task Setup(IProgress<string> progress)
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
                // TODO: Start process to download new APP update
                // TODO: Also remember to change the "algo" parameter
            }

            // Check for Miners Updates and download if so
            await UpdateMiners(progress);
        }

        public bool VersionCheck(LocalData d, Network n)
        {
            return (d.version != n.data.app_data.version);
        }

        public async Task UpdateMiners(IProgress<string> progress)
        {
            // If some miners have updates
            ToInstall[] toUpdate = MinerCheck(data, net);
            if (toUpdate.Length > 0)
            {
                // Install those updates
                await InstallMiners(toUpdate, progress);
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

        public async Task InstallMiners(ToInstall[] install, IProgress<string> progress)
        {
            string[] paths = new string[install.Length];
            string[] urls = new string[install.Length];
            string[] ending = new string[install.Length];

            // Create an array of every URL to download and every path so save to
            for (int i = 0; i < install.Length; i++)
            {
                paths[i] = install[i].path;
                urls[i] = install[i].data.GetURLFromPlatform(install[i].platform);
                ending[i] = install[i].data.file_ending;
            }

            progress.Report("Downloading miners ");

            // Batch download all the files
            await net.DownloadFiles(urls, paths, ending);

            progress.Report("Installing miners");

            // Register them as downloaded/updated
            for (int i = 0; i < install.Length; i++)
            {
                data.installed[install[i].id] = new LocalData.Miner()
                {
                    id = install[i].id,
                    path = paths[i].Replace(ending[i], ""),
                    platform = install[i].platform,
                    version = install[i].data.version,
                    algo = install[i].data.algo
                };
            }

            progress.Report("Finished installing!");
        }
        
        // If an Install is required for the given id, return a ToInstall Object
        public ToInstall InstallRequired(string id)
        {
            if (net.data.miners[id] == null)
            {
                Console.WriteLine("Asking for miner with id " + id + " but does not exist...");
                return null;
            }

            // If not currently installed or not updated, return it
            if (!data.IsInstalled(id) || ( data.installed[id] != null && data.installed[id].version != net.data.miners[id].version) || !Directory.Exists(data.installed[id].path))
            {
                // TODO: Make a system so it autodetects platform (& alt)
                string platform = "win";
                string path = Path.GetDataDirectory() + id + "." + net.data.miners[id].file_ending;

                ToInstall minerToAdd = new ToInstall(net.data.miners[id], id, platform, path);

                return minerToAdd;
            }

            return null;
        }

    }
}
