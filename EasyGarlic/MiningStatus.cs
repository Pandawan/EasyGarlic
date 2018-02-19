using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class MiningStatus {
        public string info;

        public string hashRate;
        public string lastBlock;
        public int acceptedShares;
        public int rejectedShares;
        public string temperature;

        public IProgress<MiningStatus> progress;
    }
}
