using AsyncHelper;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
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
                return host + ":" + port;
            }
        }

        private static Logger logger = LogManager.GetLogger("APIConnectLogger");

        private ConnectionInfo connectionInfo;
        private MiningStatus status;
        private AsyncTcpClient client;

        private TaskCompletionSource<bool> tcs;
        // Time (in ms) to wait before actually starting to connect
        private const int initialDelay = 2000;

        public APIConnect(ConnectionInfo _connectionInfo)
        {
            connectionInfo = _connectionInfo;
            client = new AsyncTcpClient();
            tcs = new TaskCompletionSource<bool>();

            // TODO: Make it work for AMD
        }

        public Task SetupConnection()
        {
            if (connectionInfo.IsValid())
            {
                try
                {
                    // TODO: API CRASHES HERE BECAUSE COULD NOT CONNECT
                    // System.Net.Sockets.SocketException: No connection could be made because the target machine actively refused it
                    client.ConnectAsync(connectionInfo.host, connectionInfo.port, connectionInfo.ssl);
                }
                catch (Exception e)
                {
                    logger.Error("An error occured while connecting to the miner's API");
                    logger.Error(e);

                    tcs.SetResult(false);
                }
            }

            return tcs.Task;
        }

        public async Task Stop()
        {
            if (client.IsConnected)
            {
                await client.CloseAsync();
            }

            tcs.SetResult(true);
        }

        public void SendRequest(APIConnect.Request request)
        {
            logger.Info("Connecting to " + String.Join(", ", connectionInfo));

            string message = "summary";
            // in json  string json = "{\"command\":\"" + request.command + "\",\"parameter\":\"" + request.parameter + "\"}";

        }

        public void SetStatus(MiningStatus _status)
        {
            status = _status;
        }
    }
}
