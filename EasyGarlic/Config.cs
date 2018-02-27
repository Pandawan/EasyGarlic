using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public static class Config {

        public const string VERSION = "1.0.0";
        public const string DATA_URL = "https://gist.githubusercontent.com/PandawanFr/09f294b552cb8b9a81170ceee20efdf7/raw/";
        public const string NO_ZIP_KW = "NONE";

# if DEBUG
        public const string EXTRA_PATH = @"debug\";
# else
        public const string EXTRA_PATH = "";
# endif

    }
}
