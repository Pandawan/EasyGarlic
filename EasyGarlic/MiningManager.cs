using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class MiningManager {
        public List<string> minersToUse = new List<string>();
        public string stratum;
        public string address;

        private InstallManager install;
        private IProgress<string> progress;

        public MiningManager(InstallManager im)
        {
            install = im;
        }

        public void ToggleMiner(string id)
        {
            if (minersToUse.Contains(id))
            {
                minersToUse.Remove(id);
            }
            else
            {
                minersToUse.Add(id);
            }
        }

        public async Task StartMining(string _address, string _stratum, Progress<string> _progress)
        {
            stratum = _stratum;
            address = _address;
            progress = _progress;

            // Start Mining message
            string toReturn = "Starting miner with";
            for (int i = 0; i < minersToUse.Count; i++)
            {
                toReturn += " " + minersToUse[i];
            }

            Console.WriteLine(toReturn);

            // Update any installer that needs to be installed
            await CheckInstall();

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < minersToUse.Count; i++)
            {
                tasks.Add(RunMiner(minersToUse[i]));
            }

            await Task.WhenAll(tasks.ToArray());
        }

        public async Task CheckInstall()
        {
            // Get a list of all miners that need to be installed/updated
            List<InstallManager.ToInstall> installChain = new List<InstallManager.ToInstall>();
            for (int i = 0; i < minersToUse.Count; i++)
            {
                Console.WriteLine("Checking Install for " + minersToUse[i]);
                InstallManager.ToInstall toInstall = install.InstallRequired(minersToUse[i]);
                if (toInstall != null)
                {
                    Console.WriteLine("Needed to install it");
                    installChain.Add(toInstall);
                }
            }

            // Download them
            if (installChain.Count > 0)
            {
                progress.Report("Installing miners...");
                await install.InstallMiners(installChain.ToArray(), progress);
            }
        }

        public async Task RunMiner(string id)
        {
            Command cmd = new Command();
            cmd.Setup();

            await cmd.Run(MinerCommand(install.data.installed[id]));
        }

        public string MinerCommand(LocalData.Miner miner)
        {
            string extraParams = install.data.extraParams.ContainsKey(miner.id) ? install.data.extraParams[miner.id] : "";

            if (miner.id == "nvidia")
            {
                return "ccminer-x64 --algo=" + miner.algo + " -o " + stratum + " -u " + address + " --max-temp=85 " + extraParams;
            }
            else if (miner.id == "cpu")
            {
                return "cpuminer-gw64-core2.exe -a " + miner.algo + " -o " + stratum + " -u " + address + " " + extraParams;
            }

            // TODO: ADD AMD SUPPORT

            return "RUN MINER DEFAULT";
        }

    }
}
