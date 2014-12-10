using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlickUpdater
{
    /*//////////////////////////////////////////////////////
     * This class containse misc functions / utilites for
     * slickupdater. Example of stuff in here could be:
     * a check class for dependency's or .net
     * 
     * This class is still WIP
     *//////////////////////////////////////////////////////

    class util
    {
        public static bool checkDependencies()
        {
            bool foundall = false; 

            string[] deps =
            {
                "7z.dll",
                "Newtonsoft.Json.dll",
                "SevenZipSharp.dll"
            };

            int filesFound = 0;

            string[] rawFiles = Directory.GetFiles(Directory.GetCurrentDirectory());
            List<string> filteredFiles = new List<string>();
            foreach (string file in rawFiles)
            {
                if (file.Contains(".dll"))
                {
                    var newFile = file.Replace(Directory.GetCurrentDirectory() + "\\", "");
                    filteredFiles.Add(newFile);
                }
            }

            foreach (string file in filteredFiles)
            {
                foreach (string dep in deps)
                    if (file == dep)
                        filesFound++;
            }

            if (filesFound == deps.Length)
                foundall = true;
            logit.add("Found all dependencies: " + foundall);
            return foundall;
        }
    }
}
