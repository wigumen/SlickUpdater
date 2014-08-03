using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlickUpdater
{
    public class logIt
    {
        const string logFileName = "log.txt";

        public static string logData = "Logging @ " + Path.GetFullPath(logFileName) + Environment.NewLine;
        public static StreamWriter logfile = File.AppendText(Path.GetFullPath(logFileName));

        public static void add(string log)
        {
            string logLine = "[" + DateTime.UtcNow + "] " + log + Environment.NewLine;
            logData = logData + logLine;

            logfile.Write(logLine);
            // print directly to file, not perfect but needed in case the application crashes and buffer is lost
            logfile.Flush();
        }
    }
}
