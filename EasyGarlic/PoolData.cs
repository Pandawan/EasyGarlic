using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class PoolData {

        public static PoolData Custom = new PoolData() {
            id = -1,
            name = "Custom",
            website = "",
            stratum = new string[] { "" }
        };

        public int id;
        private string _name;
        public string name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
            }
        }
        public string website;
        public string[] stratum;

        public string Value()
        {
            return stratum[0];
        }
        
        public override string ToString()
        {
            return name + ": " + JsonConvert.SerializeObject(this);
        }
    }
}
