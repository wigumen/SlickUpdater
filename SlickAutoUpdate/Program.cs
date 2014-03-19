using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace SlickAutoUpdate
{
    class Program
    {
        static string[] localversion;
        static WebClient client = new WebClient();
        static string version;
        static void Main(string[] args)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + "localversion"))
            {
                localversion = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + "localversion");
                //Console.WriteLine("Found localversion");
            }
            else { 
                Console.WriteLine("Did not find local version at " + Directory.GetCurrentDirectory() + "\\" + "localversion"); 
            }
            
            try
            {
                version = reader.webRead("http://projectawesomemodhost.com/beta/repo/slickupdater/slickversion");
            } catch (WebException e) {
                Console.WriteLine("ERROR: Could not locate web server");
            }
            if (version != "")
            {
                string[] versions = version.Split('%');


                if (versions[0] == localversion[0])
                {
                    Console.WriteLine("All is up to date so why are you launching this again?");
                }

                if (versions[0] != localversion[0])
                {
                    Console.WriteLine("Found a new version of slick updater downloading now...");
                    client.DownloadFile(versions[1], "newSlickVersion.zip");
                    Console.WriteLine("Ok downloaded the new version just have to extract it now");
                    SlickUpdater.Unzippy.extract("newSlickVersion.zip", Directory.GetCurrentDirectory());
                    File.Delete("newSlickVersion.zip");
                    Console.WriteLine("Ok its all updated killing this thread in 3 secs");
                }
            }
            System.Threading.Thread.Sleep(3000);
        }
    }
}
