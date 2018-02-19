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
            // 2018-02-18 21:44:31 | Info | Output from nvidia_win process: [2018-02-18 21:44:31][01;37m accepted: 1/1 (diff 0.022), 1060.33 kH/s [32myes![0m
            // Nvidia Regex Accepted: /(?:\[[0-9-: ]+\])(?:.*)(?:accepted: )(\d+)(?:\/)(\d+)(?:.*diff )(\d+\.?\d+)(?:.*, )([\d\.]+ .+H\/s)/

            // 2018-02-18 21:43:23 | Info | Output from nvidia_win process: [2018-02-18 21:43:23] GPU #0: 1414 MHz 23.72 kH/W 44W 37C FAN 0%[0m
            // Nvidia Regex Regular: (?:\[[0-9-: ]+\])(?:.*: )([\d]+.+Hz)(?: )([\d]+.+H\/W)(?: )([\d]+W)(?: )([\d]+C)(?: FAN )([\d]+%)
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
                    logger.Info("Output from " + id + " process: " + e.Data);

                    // TODO: Add sgminer & cpuminer support

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

                        // 0 = all, 1 = card rate, 2 = H/W, 3 = W, 4 = Temperature (C), 5 = Fan %
                        Match regularMatch = Regex.Match(e.Data, @"(?:\[[0-9-: ]+\])(?:.*: )([\d]+.+Hz)(?: )([\d]+.+H\/W)(?: )([\d]+W)(?: )([\d]+C)(?: FAN )([\d]+%)");
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
