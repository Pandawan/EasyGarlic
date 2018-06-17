using AsyncHelper;
using NLog;
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
        private CancellationTokenSource receiveCancel;

        /// <summary>
        /// This client doesn't have a documentation, but you can find the (simple to follow) class here 
        /// https://github.com/sethcall/async-helper/blob/master/src/AsyncTcpClient/AsyncTcpClient.cs
        /// </summary>
        private AsyncTcpClient client;

        private TaskCompletionSource<bool> tcs;

        public APIConnect(ConnectionInfo _connectionInfo)
        {
            logger.Warn("Constructor APIConnect");
            connectionInfo = _connectionInfo;
            client = new AsyncTcpClient();
            tcs = new TaskCompletionSource<bool>();
            cts = new CancellationTokenSource();

            // TODO: Make it work for AMD
        }

        public Task SetupConnection()
        {
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
                        await client.ConnectAsync(connectionInfo.host, connectionInfo.port, connectionInfo.ssl, cts.Token);
                        client.OnDisconnected += Client_OnDisconnected;

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

        private void Client_OnDisconnected(object sender, EventArgs e)
        {
            logger.Warn("Client disconnected!");
        }

        public async Task RefreshData(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(refreshRate, cancellationToken);
                await SendRequest(new Request("summary", ""));

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        public async Task Stop()
        {
            logger.Warn("Sent Stop to APIConnect");

            if (client.IsConnected)
            {
                cts.Cancel();
                receiveCancel.Cancel();
                await client.CloseAsync();
            }

            tcs.SetResult(true);
        }

        public async Task SendRequest(APIConnect.Request request)
        {
            logger.Warn("Sending request " + request);

            logger.Warn("Is client connected? " + client.IsConnected);

            // If client isn't connected, no point in trying to send
            if (!client.IsConnected)
            {
                logger.Warn("Could not send request. Client is not connected.");
                return;
            }

            // in json  string json = "{\"command\":\"" + request.command + "\",\"parameter\":\"" + request.parameter + "\"}";

            await client.SendAsync(Encoding.ASCII.GetBytes(request.command));
            receiveCancel = new CancellationTokenSource();
            client.OnDataReceived += Client_OnDataReceived;
            try
            {
                await client.Receive(receiveCancel.Token);
            }
            // Ignore token cancellation
            catch (Exception e) when (e is OperationCanceledException || e is InvalidOperationException) { }
            
        }

        private void Client_OnDataReceived(object sender, byte[] e)
        {
            logger.Warn("Received " + Encoding.ASCII.GetString(e));
            // After receiving cancel immediately, need to send a new request
            receiveCancel.Cancel();
        }

        public void SetStatus(MiningStatus _status)
        {
            status = _status;
        }
    }
}
