using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCompress;
using NLog;

namespace EasyGarlic {
    public class MinerManager {

        private static Logger logger = LogManager.GetLogger("MiningLogger");

        public LocalData data;
        private Linker linker;

        public async Task Setup(Linker _linker, IProgress<ProgressReport> progress)
        {
            linker = _linker;
            logger.Info("Loading Local Data...");
            progress.Report(new ProgressReport("Loading Local Data..."));
            // Create or Load LocalData
            data = new LocalData();
            if (data.Exists())
            {
                data = await LocalData.LoadAsync();
            }
            else
            {
                data.SetupDefault();
            }

            // TODO: Save user data such as pool, address, and which miners were enabled previously
        }

        public async Task StartMining(string address, string pool, IProgress<MiningStatus> progress)
        {
            // TODO: Add system so that each GPU has its own mining status and therefore, its own miner info
            MiningStatus status = new MiningStatus();
            status.progress = progress;

            string[] minersToUse = GetMinersWithStatus(MinerStatus.Enabled);

            status.info = "Starting miners " + String.Join(", ", minersToUse);
            logger.Info(status.info);
            progress.Report(status);
            
            List<Task> miningTasks = new List<Task>();

            for (int i = 0; i < minersToUse.Length; i++)
            {
                Miner miner = data.installed[minersToUse[i]];

                // Custom status for the miner itself
                MiningStatus minerStatus = new MiningStatus();
                status.progress = progress;
                status.id = miner.GetID();

                // If the miner has a problem
                if (!(await VerifyMiner(miner)))
                {
                    status.info = "Miner \"" + miner.GetID() + "\" could not be found, please try reinstalling it.";
                    logger.Error(status.info);
                    progress.Report(status);
                    return;
                }

                string commandToRun = GetMinerCommand(address, pool, miner);
                // Setup Command Process
                miner.miningProcess = new Command();
                miner.miningProcess.Setup(minersToUse[i], true);
                miner.miningProcess.SetStatus(status);

                // Set Status as Mining
                miner.status = MinerStatus.Mining;

                // Run the mining command
                miningTasks.Add(miner.miningProcess.Run(commandToRun));

                // TODO: Add support for multiple GPUs to work together (GPU list by ID and get GPU type, then start Command for each of them)
                // TODO: Add a system to return Progress reports to display hashrate + balance + blocks mined...
            }

            status.info = "Mining...";
            logger.Info(status.info);
            progress.Report(status);
            await Task.WhenAll(miningTasks);

            status.info = "Finished mining.";
            logger.Info(status.info);
            progress.Report(status);
        }

        public async Task StopMining(IProgress<string> progress)
        {
            string[] minersToStop = GetMinersWithStatus(MinerStatus.Mining);

            if (minersToStop.Length == 0)
            {
                logger.Info("No miners to stop.");
                progress.Report("No miners to stop.");
                return;
            }
            
            logger.Info("Stopping all miners...");
            progress.Report("Stopping all miners...");

            List<Task> stoppingTasks = new List<Task>();

            for (int i = 0; i < minersToStop.Length; i++)
            {
                stoppingTasks.Add(data.installed[minersToStop[i]].miningProcess.Stop());
            }

            await Task.WhenAll(stoppingTasks);

            for (int i = 0; i < minersToStop.Length; i++)
            {
                data.installed[minersToStop[i]].status = MinerStatus.Enabled;
            }

            logger.Info("Stopped all miners. Ready!");
            progress.Report("Stopped all miners. Ready!");
        }

        public async Task<bool> VerifyMiner(Miner miner)
        {
            bool exists = await Task.Run(() => { return File.Exists(miner.installPath + miner.fileNameMine); });

            return exists;
        }

        public async Task EnableMiner(string id, IProgress<bool> progress, IProgress<string> installing)
        {
            string realId = id + "_" + GetCurrentPlatform();
            // TODO: Add auto miner update
            if (data.installed.ContainsKey(realId))
            {
                // If we're using the alternate miner
                if (data.installed[realId].usingAlt)
                {
                    // Check that it is installed
                    if (data.installed.ContainsKey(realId + "_alt"))
                    {
                        data.installed[realId + "_alt"].status = MinerStatus.Enabled;
                        logger.Info("Enabled miner " + realId + "_alt");
                    }
                    // If not yet installed, install it now
                    else
                    {
                        progress.Report(false);
                        await InstallMiner(realId + "_alt", installing);
                        data.installed[realId + "_alt"].status = MinerStatus.Enabled;

                        progress.Report(true);
                        logger.Info("Enabled miner " + realId + "_alt");
                    }
                }
                // If using the default miner
                else
                {
                    data.installed[realId].status = MinerStatus.Enabled;
                    logger.Info("Enabled miner " + realId);
                }
                progress.Report(true);
            }
            else
            {
                // Install the miner if not installed yet
                progress.Report(false);
                await InstallMiner(realId, installing);
                data.installed[realId].status = MinerStatus.Enabled;

                progress.Report(true);
                logger.Info("Enabled miner " + realId);
            }
        }

        public void DisableMiner(string id, IProgress<bool> progress)
        {
            string realId = id + "_" + GetCurrentPlatform();

            // If the miner exists and it isn't disabled, disable it
            if (data.installed.ContainsKey(realId) && data.installed[realId].status != MinerStatus.Disabled)
            {
                data.installed[realId].status = MinerStatus.Disabled;

                logger.Info("Disabled miner " + realId);
            }

            // If the alt miner exists and it isn't disabled, disable it 
            if (data.installed.ContainsKey(realId + "_alt") && data.installed[realId + "_alt"].status != MinerStatus.Disabled)
            {
                data.installed[realId + "_alt"].status = MinerStatus.Disabled;

                logger.Info("Disabled miner " + realId + "_alt");
            }

            logger.Debug("Using miners: " + String.Join(", ", GetMinersWithStatus(MinerStatus.Enabled)));

            // Report true if there still are miners that are available to mine
            progress.Report((GetMinersWithStatus(MinerStatus.Enabled).Length > 0));
        }

        public string[] GetMinersWithStatus(MinerStatus status)
        {
            List<string> miners = new List<string>();

            foreach (KeyValuePair<string, Miner> item in data.installed)
            {
                if (item.Value.status == status)
                {
                    miners.Add(item.Key);
                }
            }

            return miners.ToArray();
        }

        public async Task InstallMiner(string id, IProgress<string> progress)
        {
            OnlineData onlineData = linker.networkManager.data;

            // Don't want to use Online Data if it doesn't exist
            if (onlineData == null)
            {
                progress.Report("Could not fetch Online Data when installing");
                return;
            }

            // TODO: Might want to add a bunch of try catch for download and zip

            if (onlineData.miners.ContainsKey(id) && onlineData.miners[id] != null)
            {
                // Get data for miner to install
                OnlineMiner minerToInstall = onlineData.miners[id];
                string downloadPath = Path.GetDataDirectory() + minerToInstall.fileNameZip;
                // If it's a direct zip, use direct file downloadPath
                if (minerToInstall.fileNameZip == Config.NO_ZIP_KW)
                {
                    downloadPath = Path.GetDataDirectory() + minerToInstall.fileNameMine;
                }

                logger.Info("Downloading miner \"" + id + "\"");
                progress.Report("Downloading miner \"" + id + "\"");

                // Download the file async
                await linker.networkManager.DownloadFileAsync(downloadPath, minerToInstall.downloadURL);

                logger.Info("Installing miner \"" + id + "\"");
                progress.Report("Installing miner \"" + id + "\"");

                // Get Install Path
                string installPath = Path.GetMinerDirectory(id);
                minerToInstall.installPath = installPath;

                // If the file isn't a zip, create a folder instead
                if (minerToInstall.fileNameZip == Config.NO_ZIP_KW)
                {
                    await linker.networkManager.InstallInFolder(downloadPath, installPath);
                }
                // If the file IS a zip, extract it
                else
                {
                    // Extract the zip async
                    await linker.networkManager.ExtractZipFile(downloadPath, installPath);
                }
                // Add it to list of installed
                data.installed.Add(id, minerToInstall);

                // Save file
                await data.SaveAsync();

                logger.Info("Finished installing miner \"" + id + "\"");
                progress.Report("Finished installing miner \"" + id + "\"");
            }
            else
            {
                logger.Error("No miner found with id " + id);
                progress.Report("No miner found with id " + id);
            }
        }

        public async Task UninstallMiner(Miner miner, IProgress<string> progress)
        {
            logger.Info("Uninstalling miner \"" + miner.GetID() + "\"");
            progress.Report("Uninstalling miner \"" + miner.GetID() + "\"");

            string downloadPath = Path.GetDataDirectory() + miner.fileNameZip;
            // If it's a direct zip, use direct file downloadPath
            if (miner.fileNameZip == Config.NO_ZIP_KW)
            {
                downloadPath = Path.GetDataDirectory() + miner.fileNameMine;
            }

            await Task.Run(() =>
            {
                try
                {
                    // Delete ZIP/Downloaded file
                    if (File.Exists(downloadPath))
                    {
                        File.Delete(downloadPath);
                    }

                    // Delete Install Path
                    if (Directory.Exists(miner.installPath))
                    {
                        Directory.Delete(miner.installPath, true);
                    }
                }
                catch(Exception e)
                {
                    logger.Error("ERROR while deleting files & folders");
                    logger.Error(e);
                    progress.Report("Something went wrong. Please try again later.");
                }
            });

            data.installed.Remove(miner.GetID());
            await data.SaveAsync();
            
            logger.Info("Finished uninstalling miner \"" + miner.GetID() + "\"");
            progress.Report("Finished uninstalling miner \"" + miner.GetID() + "\"");

            logger.Info("Installed: " + String.Join(", ", data.installed.Keys.ToArray()));
        }

        public string GetMinerCommand(string address, string pool, Miner m)
        {
            string command = "";

            // AMD needs some extra parameters
            if (m.type == "amd")
            {
                command += "@echo off \n\nsetx GPU_FORCE_64BIT_PTR 0 \nsetx GPU_MAX_HEAP_SIZE 100 \nsetx GPU_MAX_SINGLE_ALLOC_PERCENT 100 \nsetx GPU_MAX_ALLOC_PERCENT 100 \nsetx GPU_USE_SYNC_OBJECTS 1 \n";
            }

            command += "\"" + m.installPath + m.fileNameMine + "\"";

            // Algorithm is diff on amd
            if (m.type == "amd")
            {
                command += " --algorithm " + m.algo;
            }
            else
            {
                command += " -a " + m.algo;
            }

            command += " -o " + pool + " -u " + address + " " + m.extraParameters;

            // Custom intensity if not 0
            if (m.type == "nvidia" && m.customIntensity != 0)
            {
                command += "-i " + m.customIntensity;
            }
            else if (m.type == "amd" && m.customIntensity != 0)
            {
                command += "-I " + m.customIntensity;
            }

            // Algorithm is diff on amd
            if (m.type == "amd")
            {
                // Add debug for amd
                command += " --debug ";
            }

            // Add custom parameters last
            command += m.customParameters;
            
            return command;
        }

        public string GetCurrentPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return "win";
                // TODO: Add Mac support
                case PlatformID.MacOSX: 
                    return "";
                case PlatformID.Unix:
                    return "nix";
            }

            logger.Error("Platform not supported...");
            return "";
        }

        public string GetSavedAddress()
        {
            if (String.IsNullOrWhiteSpace(data.savedAddress))
            {
                return "";
            }
            else
            {
                return data.savedAddress;
            }
        }

        public void SaveAddress(string value)
        {
            data.savedAddress = value;
        }

        public string GetDataURL()
        {
            return data.dataURL;
        }

    }
}
