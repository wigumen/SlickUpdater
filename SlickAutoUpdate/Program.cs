using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SlickAutoUpdate
{
    class Program
    {
        static string[] localversion;
        static WebClient client = new WebClient();
        static versionfile slickversion;
        static void Main(string[] args)
        {
            string rawSlickJson = reader.webRead("http://arma.projectawesome.net/beta/repo/slickupdater/slickversion.json");
            slickversion = JsonConvert.DeserializeObject<versionfile>(rawSlickJson);

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
            } catch (WebException e) {
                Console.WriteLine("ERROR: Could not locate web server");
            }
            if (rawSlickJson != null)
            {


                if (slickversion.version == localversion[0])
                {
                    Console.WriteLine("SlickUpdater already is up-to-date.");
                }

                if (slickversion.version!= localversion[0])
                {
                    Console.WriteLine("Found an updated version of SlickUpdater, downloading now...");
                    client.DownloadFile(slickversion.download, "newSlickVersion.zip");
                    Console.WriteLine("Extracting download...");
                    SlickUpdater.Unzippy.extract("newSlickVersion.zip", Directory.GetCurrentDirectory());
                    File.Delete("newSlickVersion.zip");
                    Console.WriteLine("Update succes, killing process...");
                }
            }
            System.Threading.Thread.Sleep(3000);
        }
    }
}
