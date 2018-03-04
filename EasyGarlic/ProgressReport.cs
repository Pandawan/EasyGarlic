using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class ProgressReport {
        public string message;
        public Exception error;

        public ProgressReport(string _message)
        {
            message = _message;
            error = null;
        }

        public ProgressReport(string _message, Exception _error)
        {
            message = _message;
            error = _error;
        }
    }

}
