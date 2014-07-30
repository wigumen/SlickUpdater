using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlickUpdater
{
    public class logIt
    {
        public static string logData = "";

        public static void addData(string log)
        {
            logData = logData + "[" + DateTime.UtcNow + "] " + log + "\r";
        }
    }
}
