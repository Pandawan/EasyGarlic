using System.Net;

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

        private string connectURL;

        public APIConnect()
        {
            connectURL = "127.0.0.1:4028";

            // TODO: Make it work for AMD
        }

        public void SendRequest(APIConnect.Request request)
        {
            //string json = JsonConvert.SerializeObject(request);
            string json = "{\"command\":\"" + request.command + "\",\"parameter\":\"" + request.parameter + "\"}";

            using (var webClient = new WebClient())
            {
                var response = webClient.UploadString(connectURL, "POST", json);
            }
        }

    }
}
