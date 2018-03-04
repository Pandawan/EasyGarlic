using System;

namespace EasyGarlic {
    public class MiningStatus {
        public string info;

        public string id;

        public string hashRate;
        public string lastBlock;
        public int acceptedShares;
        public int rejectedShares;
        public string temperature;

        public IProgress<MiningStatus> progress;
    }
}
