using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace EasyGarlic {
    public class NLogText : Target {
        public Action<LogEventInfo> Log = delegate { };

        public NLogText(string name, LogLevel level)
        {
            LogManager.Configuration.AddTarget(name, this);

            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", level, this));//This will ensure that exsiting rules are not overwritten
            LogManager.Configuration.Reload(); //This is important statement to reload all applied settings

            //SimpleConfigurator.ConfigureForTargetLogging (this, level); //use this if you are intending to use only NlogMemoryTarget  rule
        }

        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            foreach (var logEvent in logEvents)
            {
                Write(logEvent);
            }
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write(logEvent.LogEvent);

        }

        protected override void Write(LogEventInfo logEvent)
        {
            Log(logEvent);
        }
    }
}
