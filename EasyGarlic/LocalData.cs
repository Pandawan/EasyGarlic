using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class LocalData {

        public class Miner {
            public string id;
            public string platform;
            public string version;
            public string path;
        }

        private string path = Path.GetDataDirectory() + "data.json";

        public string version;
        public Dictionary<string, Miner> installed = new Dictionary<string, Miner>();
        
        public void Save()
        {
            IO.SaveAsJson(path, this);
        }

        public void Load()
        {
            LocalData d = IO.ReadTo<LocalData>(path);

            // If there was a previous data, load it
            if (d != null)
            {
                path = d.path;
                version = d.version;
                installed = d.installed;
            }
            // If there wasn't, clear everything
            else
            {
                version = Config.VERSION;
                path = Path.GetDataDirectory() + "data.json";
                installed = new Dictionary<string, Miner>();
            }
        }
    }
}
