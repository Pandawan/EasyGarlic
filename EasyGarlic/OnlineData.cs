using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class OnlineData {
        
        public string pools;
        public Dictionary<string, OnlineMiner> miners = new Dictionary<string, OnlineMiner>();
        
        // Loads the given JSON data into a LocalData object
        public static OnlineData LoadData(string jsonData)
        {
            OnlineData data = JsonConvert.DeserializeObject<OnlineData>(jsonData);
            
            return data;
        }
    }
}
