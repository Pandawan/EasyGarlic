using NLog;
using WatsonTcp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class APIConnect {

        public struct Request {
            public string command;
            public string parameter;

            public Request(string _command, string _parameter)
            {
                command = _command;
                parameter = _parameter;
            }

            public override string ToString()
            {
                return "command: " + command + " | parameter: " + parameter;
            }
        }

        public struct ConnectionInfo {
            public string host;
            public int port;
            public bool ssl;

            public ConnectionInfo(string _host, int _port)
            {
                host = _host;
                port = _port;
                ssl = false;
            }

            public ConnectionInfo(string _host, int _port, bool _ssl)
            {
                host = _host;
                port = _port;
                ssl = _ssl;
            }

            public bool IsValid()
            {
                return (!String.IsNullOrEmpty(host) && port != 0);
            }

            public override string ToString()
            {
                return host + ":" + port + " | SSL: " + ssl;
            }
        }

        private static Logger logger = LogManager.GetLogger("APIConnectLogger");

        private ConnectionInfo connectionInfo;
        private MiningStatus status;

        private int refreshRate = 5000;
        private CancellationTokenSource cts;

        /// <summary>
        /// This client doesn't have a documentation, but you can find the (simple to follow) class here 
        /// https://github.com/sethcall/async-helper/blob/master/src/AsyncTcpClient/AsyncTcpClient.cs
        /// </summary>
        private WatsonTcpClient client;

        private TaskCompletionSource<bool> tcs;

        public APIConnect(ConnectionInfo _connectionInfo)
        {
            logger.Warn("Constructor APIConnect");
            connectionInfo = _connectionInfo;
            tcs = new TaskCompletionSource<bool>();
            cts = new CancellationTokenSource();

            // TODO: Make it work for AMD
        }

        public Task TSetupConnection()
        {
            logger.Warn("Connecting to API " + connectionInfo);

            if (connectionInfo.IsValid())
            {
                Task.Run(async () =>
                {
                    // Wait a bit for the API to start
                    await Task.Delay(refreshRate, cts.Token);

                    // Create new TCP client
                    client = new WatsonTcpClient(connectionInfo.host, connectionInfo.port, Client_OnConnected, Client_OnDisconnected, Client_OnMessageReceived, false);
                    
                    // Start refresh loop
                    // await RefreshData(cts.Token);
                    await SendRequest(new Request("summary", ""));

                    logger.Warn("Done!");
                });
            }

            logger.Warn("AYAYAY");

            return tcs.Task;
        }

        public Task SetupConnection()
        {
            return TSetupConnection();

            logger.Warn("Connecting to API " + connectionInfo);

            if (connectionInfo.IsValid())
            {
                // TODO: Find a way to WAIT for the miner to open its port before starting the API Connection
                // Perhaps call SetupConnection on keyword identification in the Command.cs file (where it parses each line)
                Task.Run(async () =>
                {
                    // Wait a bit for the API to boot
                    await Task.Delay(refreshRate);

                    try
                    {
                        client = new WatsonTcpClient(connectionInfo.host, connectionInfo.port, Client_OnConnected, Client_OnDisconnected, Client_OnMessageReceived, true);
                        await RefreshData(cts.Token);

                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occured while connecting to the miner's API");
                        logger.Error(e);

                        tcs.SetResult(false);
                    }
                });
            }

            return tcs.Task;
        }

        private bool Client_OnConnected()
        {
            logger.Warn("Client connected!");
            return true;
        }

        private bool Client_OnDisconnected()
        {
            logger.Warn("Client disconnected!");
            return true;
        }

        private bool Client_OnMessageReceived(byte[] data)
        {
            logger.Warn("Received message " + Encoding.ASCII.GetString(data));
            return true;
        }

        public void TRefreshData(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                    Thread.Sleep(refreshRate);
                    TSendRequest(new Request("summary", ""));
            }
        }

        public async Task RefreshData(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(refreshRate, cancellationToken);
                    await SendRequest(new Request("summary", ""));
                }
                catch (TaskCanceledException e) { }
            }
        }

        public void TSendRequest(APIConnect.Request request)
        {
            logger.Warn("Sending request " + request);

            logger.Warn("Is client connected? " + client.IsConnected());

            // If client isn't connected, no point in trying to send
            if (!client.IsConnected())
            {
                logger.Warn("Could not send request. Client is not connected.");
                return;
            }

            bool val = client.Send(Encoding.ASCII.GetBytes(request.command));
            logger.Warn("Output " + val);
        }

        public async Task SendRequest(APIConnect.Request request)
        {
            logger.Warn("Sending request " + request);

            logger.Warn("Is client connected? " + client.IsConnected());

            // If client isn't connected, no point in trying to send
            if (!client.IsConnected())
            {
                logger.Warn("Could not send request. Client is not connected.");
                return;
            }

            bool val = await client.SendAsync(Encoding.ASCII.GetBytes(request.command));
            logger.Warn("Output " + val);
        }

        public void Stop()
        {
            logger.Warn("Sent Stop to APIConnect");

            if (cts != null)
            {
                // Cancel just in case
                cts.Cancel();
                logger.Warn(cts.IsCancellationRequested);
            }

            // Dispose of connection if still up
            if (client.IsConnected())
            {
                client.Dispose();
            }

            tcs.SetResult(true);
        }

        public void SetStatus(MiningStatus _status)
        {
            status = _status;
        }
    }
}
