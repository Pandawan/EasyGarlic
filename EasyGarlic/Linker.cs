using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class Linker {

        public NetworkManager networkManager;
        public MinerManager minerManager;
        
        public async Task Setup(IProgress<string> progress)
        {
            // Create a MinerManager & Setup
            minerManager = new MinerManager();
            await minerManager.Setup(this, progress);

            // Create a NetworkManager & Setup
            networkManager = new NetworkManager();
            await networkManager.Setup(this, progress);

            // TODO: Add miner auto-update on start
            
            // Save once startup is done
            await minerManager.data.SaveAsync();
        }

        public static string IDToTitle(string id)
        {
            if (id.Contains("nvidia"))
            {
                return "Nvidia GPU";
            }

            if (id.Contains("amd"))
            {
                return "AMD GPU";
            }

            if (id.Contains("cpu"))
            {
                return "CPU";
            }

            return "";
        }

    }
}
