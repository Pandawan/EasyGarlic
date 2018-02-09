using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class ApiData {
        
        public class MinerData {
            public string version;
            public string file_ending;
            public string win;
            public string win_alt;
            public string nix;
            public string nix_alt;
            public string algo;

            public string GetURLFromPlatform(string platform)
            {
                switch(platform)
                {
                    case "win":
                        return win;
                    case "win_alt":
                        return win_alt;
                    case "nix":
                        return nix;
                    case "nix_alt":
                        return nix_alt;
                    default:
                        return null;
                }
            }
        }

        public MinerData app_data;

        public string pools;
        public Dictionary<string, MinerData> miners = new Dictionary<string, MinerData>();
    }
}
