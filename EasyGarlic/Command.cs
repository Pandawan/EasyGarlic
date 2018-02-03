using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace EasyGarlic {
    public class Command {

        private ProcessStartInfo startInfo;
        private Process cmd;

        private bool startReading = false;
        private bool calledStop = false;

        public void Setup()
        {
            Console.WriteLine("Initializing Command Processes...");

            // Create Start Info to pass to the Process Starter
            startInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
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

            // Begin reading async
            cmd.BeginErrorReadLine();
            cmd.BeginOutputReadLine();

            Console.WriteLine("Command Processes are Ready");

            // Start reading
            cmd.StandardInput.WriteLine("echo START");
        }

        public void Stop()
        {
            Console.WriteLine("Terminating Command Processes...");

            calledStop = true;

            //cmd.StandardInput.WriteLine("exit");

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            Console.WriteLine("Command Processes are Stopped");
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                // Only start reading the data once START was reached
                if (e.Data == "START")
                {
                    startReading = true;
                }

                // If already started reading, read it
                if (startReading)
                {
                    Console.WriteLine("Output from other process: " + e.Data);
                }
            }
        }

        private void OnError(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data) && startReading)
            {
                Console.WriteLine("Output from other process: " + e.Data);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (!calledStop)
            {
                Console.WriteLine("Unexpectedly stopped the process...");
            }
        }
    }
}
