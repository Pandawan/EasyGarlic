using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Management;
using NLog;
using System.Text.RegularExpressions;

namespace EasyGarlic {
    public class Command {

        private static Logger logger = LogManager.GetLogger("CommandLogger");

        private ProcessStartInfo startInfo;
        private Process cmd;
        private TaskCompletionSource<bool> tcs;
        private MiningStatus status;

        private bool startReading = false;
        private bool calledStop = false;

        // Using string hashrate to have units
        private string totalHashrate = "";
        // List of Device # that has already been recorded (and how many times it has been recorded)
        private Dictionary<string, int> hashratesSeen = new Dictionary<string, int>();

        private string id;

        public void Setup(string _id, bool hide)
        {
            id = _id;

            tcs = new TaskCompletionSource<bool>();

            logger.Info("Initializing Command Processes for " + id + "...");

            // Create Start Info to pass to the Process Starter
            startInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = hide,
            };

            // Create a new process
            cmd = new Process();

            // Assign event handlers
            cmd.EnableRaisingEvents = true;
            cmd.Exited += OnExit;
            cmd.OutputDataReceived += OnDataReceived;
            cmd.ErrorDataReceived += OnError;

            // Start the process
            cmd.StartInfo = startInfo;
            cmd.Start();

            // Add process to ChildProcessTracker to force close it on close
            ChildProcessTracker.AddProcess(cmd);

            // Begin reading async
            cmd.BeginErrorReadLine();
            cmd.BeginOutputReadLine();

            logger.Info("Command Processes for " + id + " are Ready");

            // Start reading
            cmd.StandardInput.WriteLine("echo START");
        }

        public Task Run(string commandToRun)
        {
            cmd.StandardInput.WriteLineAsync(commandToRun);

            logger.Debug("Running command " + commandToRun);

            return tcs.Task;
        }

        public async Task Stop()
        {
            logger.Info("Terminating Command Processes for " + id + "...");

            calledStop = true;

            await Task.Run(() =>
            {
                cmd.StandardInput.Close();
                // Wait 1/2 a second to let it close
                bool didStop = cmd.WaitForExit(500);
                // If did not close, force kill it
                if (!didStop)
                {
                    // Kill process and child processes (running a miner uses a separate process)
                    KillProcessAndChildren(cmd.Id);
                }
            });

            logger.Info("Command Processes for " + id + " are Stopped");
        }

        public void SetStatus(MiningStatus _status)
        {

            ///  NVIDIA \\\
            // [2018-02-18 21:44:31][01;37m accepted: 1/1 (diff 0.022), 1060.33 kH/s [32myes![0m
            // Nvidia Regex Accepted: (?:\[[0-9-: ]+\])(?:.*)(?:accepted: )(\d+)(?:\/)(\d+)(?:.*diff )(\d+\.?\d+)(?:.*, )([\d\.]+ .+H\/s)

            // [2018-02-18 21:43:23] GPU #0: 1414 MHz 23.72 kH/W 44W 37C FAN 0%[0m
            // Nvidia Regex Regular: (?:\[[0-9-: ]+\])(?:.*GPU #)(\d+)(?:: )([\d]+.+Hz)(?: )([\d]+.+H\/W)(?: )([\d]+W)(?: )([\d]+C)(?: FAN )([\d]+%)

            // [2018-02-20 13:43:54][36m allium block 67129, diff 250.313[0m
            // Nvidia Regex Block: (?:\[[0-9-: ]+\])(?:.*allium block )(\d*)(?:.*diff )([\d\.]*)

            /// CPU \\\
            // [2018-02-20 13:43:59] CPU #0: 39.30 kH/s[0m      
            // (OR FOR OPT) [2018-03-02 18:40:29] CPU #2: 420.09 kH, 75.86 kH/s[0m
            // CPU Regex Regular: (?:\[[0-9-: ]+\])(?:.*CPU #)(\d+)(?:.+ )([\d]+.+H\/s)

            // [2018-02-18 21:44:31][01;37m accepted: 1/1 (diff 0.022), 1060.33 kH/s [32myes![0m
            // CPU Regex Accepted: (?:\[[0-9-: ]+\])(?:.*)(?:accepted: )(\d+)(?:\/)(\d+)(?:.*diff )(\d+\.?\d+)(?:.*, )([\d\.]+ .+H\/s)

            // [2018-02-20 13:43:56][36m allium block 67129, diff 250.313[0m
            // CPU Regex Block: (?:\[[0-9-: ]+\])(?:.*allium block )(\d*)(?:.*diff )([\d\.]*)

            status = _status;
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: Make a new GLOBAL Command Process so that any output from any of these commands is put into that new window for people that want to debug (show command window option)

            if (!String.IsNullOrEmpty(e.Data))
            {
                // Only start reading the data once START was reached
                if (e.Data == "START")
                {
                    startReading = true;
                    return;
                }

                // If already started reading, read it
                if (startReading)
                {
                    string formattedData = new Regex("(\\[[\\S]+m)").Replace(e.Data, "");
                    logger.Info("Output " + id + ": " + formattedData);
                    //logger.Info("Output from " + id + " process: " + e.Data);

                    // TODO: Add sgminer support
                    // TODO: Add up all the mining rates together for all miners rather than just CPU (which would allow multiple miner support)

                    // ccminer output
                    if (id.Contains("nvidia"))
                    {
                        // 0 = all, 1 = accepted shares, 2 = total shares, 3 = difficulty, 4 = hashrate
                        Match acceptedMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*)(?:accepted: )(\d+)(?:\/)(\d+)(?:.*diff )(\d+\.?\d+)(?:.*, )([\d\.]+ .+H\/s)");
                        if (acceptedMatch.Success)
                        {
                            // Rejected = max - accepted
                            status.rejectedShares = int.Parse(acceptedMatch.Groups[2].Value) - int.Parse(acceptedMatch.Groups[1].Value);
                            // Accepted = accepted
                            status.acceptedShares = int.Parse(acceptedMatch.Groups[1].Value);
                            status.hashRate = acceptedMatch.Groups[4].Value;
                        }

                        // 0 = all, 1 = gpu #, 2 = card rate, 3 = H/W, 4 = W, 5 = Temperature (C), 6 = Fan %
                        Match regularMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*GPU #)(\d+)(?:: )([\d]+.+Hz)(?: )([\d]+.+H\/W)(?: )([\d]+W)(?: )([\d]+C)(?: FAN )([\d]+%)");
                        if (regularMatch.Success)
                        {
                            status.temperature = regularMatch.Groups[4].Value;
                        }

                        // 0 = all, 1 = block count, 2 = diff
                        Match blockMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*allium block )(\d*)(?:.*diff )([\d\.]*)");
                        if (blockMatch.Success)
                        {
                            status.lastBlock = blockMatch.Groups[1].Value;
                        }
                    }

                    // cpuminer output
                    if (id.Contains("cpu"))
                    {
                        status.temperature = "Not Available";

                        // Hashrate
                        // 0 = all, 1 = cpu #, 2 = rate
                        Match regularMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*CPU #)(\d+)(?:.+ )([\d]+.+H\/s)");
                        if (regularMatch.Success)
                        {
                            // If that key doesn't exist, create it at 0
                            if (!hashratesSeen.ContainsKey(regularMatch.Groups[1].Value))
                            {
                                hashratesSeen.Add(regularMatch.Groups[1].Value, 0);
                            }

                            // Calculate if all have been seen once
                            Dictionary<string, int> tempCopy = new Dictionary<string, int>(hashratesSeen);
                            int numberSeen = 0;
                            foreach (KeyValuePair<string, int> item in hashratesSeen)
                            {
                                // Use 2 so that it doesn't do it on first value it gets (where dictionary is empty)
                                if (item.Value >= 1)
                                {
                                    numberSeen++;
                                    tempCopy[item.Key]--;
                                }
                            }

                            // If all have been seen at least once, set as "seen" and remove 1 from each
                            bool allSeen = (numberSeen == hashratesSeen.Count);

                            // If all have been seen, send total hashrate
                            if (allSeen)
                            {
                                // Reset everything to -1 
                                hashratesSeen = tempCopy;

                                // Send the total hashrate, then reset because it's 0 so first of next series
                                status.hashRate = totalHashrate;
                                status.progress.Report(status);

                                totalHashrate = "";
                            }

                            // Add seen + 1 for that device
                            hashratesSeen[regularMatch.Groups[1].Value] += 1;
                            // Add the hashrates and put them in as total
                            totalHashrate = Utilities.AddHashes(regularMatch.Groups[2].Value, totalHashrate);

                            // Return because we don't want to return hashrate every time, only total hashrate (which means multiple lines need to be processed before sending)
                            return;
                        }

                        // Accepted & Rejected
                        // 0 = all, 1 = accepted shares, 2 = total shares, 3 = difficulty, 4 = hashrate
                        Match acceptedMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*)(?:accepted: )(\d+)(?:\/)(\d+)(?:.*diff )(\d+\.?\d+)(?:.*, )([\d\.]+ .+H\/s)");
                        if (acceptedMatch.Success)
                        {
                            // Rejected = max - accepted
                            status.rejectedShares = int.Parse(acceptedMatch.Groups[2].Value) - int.Parse(acceptedMatch.Groups[1].Value);
                            // Accepted = accepted
                            status.acceptedShares = int.Parse(acceptedMatch.Groups[1].Value);
                        }
                        
                        // Block Value
                        // 0 = all, 1 = block count, 2 = diff
                        Match blockMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*allium block )(\d*)(?:.*diff )([\d\.]*)");
                        if (blockMatch.Success)
                        {
                            status.lastBlock = blockMatch.Groups[1].Value;
                        }
                    }
                    
                    // Report status change
                    status.progress.Report(status);
                }
            }
        }

        private void OnError(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data) && startReading)
            {
                logger.Error("Output from " + id + " process: " + e.Data);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (!calledStop)
            {
                logger.Error("Unexpectedly stopped process " + id);
            }

            tcs.SetResult(true);
        }

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }
}
