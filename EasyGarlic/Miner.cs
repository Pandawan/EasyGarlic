using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyGarlic {
    public enum MinerStatus {
        Enabled, // Set to be used for mining, but not yet mining
        Mining, // Currently mining
        Disabled // Set to not be used and not mining
    }

    public class Miner {
        
        // Basic Info
        public string type;
        public string platform;

        // Installation Info
        public string installPath;
        // File Name before extraction
        public string fileNameZip;
        // File Name after extraction
        public string fileNameMine;

        // Mining Info
        public string algo;
        public string extraParameters;

        [JsonIgnore]
        public MinerStatus status = MinerStatus.Disabled;

        [JsonIgnore]
        public Command miningProcess;
        
        public string GetID()
        {
            return type + "_" + platform;
        }

        public override string ToString()
        {
            return GetID() + ":{ " + "algo:" + algo + ", extraParams:" + extraParameters + ", fileZip: " + fileNameZip + ", fileMine: " + fileNameMine + " }"; 
        }
    }

    public class OnlineMiner : Miner {

        public string downloadURL;
    }
}
