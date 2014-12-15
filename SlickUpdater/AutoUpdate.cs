using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace SlickUpdater
{
    public class AutoUpdate
    {
        static versionfile slickversion;
        public Boolean exupdate = false;
        public void Initialize()
        {
            slickversion = JsonConvert.DeserializeObject<versionfile>(getSlickJson());
            if (slickversion == null)
            {
                //abort update because of getting a nullvalue from getSlicJson
                return;
            }
            //CheckAvailableUpdates();
        }
        public String getSlickJson()
        {
#if DEBUG
            //local debug server for testing
            rawSlickJson = downloader.webRead("http://localhost/slickversion.json");
#else
            String SlickJson = String.Empty;
            try
            {
                try
                {
                    SlickJson = downloader.webRead("http://arma.projectawesome.net/beta/repo/slickupdater/slickversion.json");
                }
                catch(Exception ex)
                {
                    //Error while trying to get SlickJson
                    logIt.add("Error while trying to download slickversion.json from PA server, trying backup on gists. Error:\n" + ex.ToString());
                }
                if (String.IsNullOrEmpty(SlickJson) == true)
                {
                    try
                    {
                        SlickJson = downloader.webRead("https://gist.githubusercontent.com/wigumen/015cb44774c6320cf901/raw/6a5f22437997c6c120a1b15beaabdb3ade3be06a/slickversion.json");
                    }
                    catch(Exception ex)
                    {
                        logIt.add("Error while trying to download slickversion.json from backup server (gists). Error:\n" + ex.ToString());
                    }
                }

                if (String.IsNullOrEmpty(SlickJson) == false)
                {
                    return SlickJson;
                }
                else
                {
                    //it seems that the slickversion.json could not be downloaded. This is REALLY bad...
                    return null;
                }
            }
            catch(Exception ex)
            {
                logIt.add("Something went terribly wrong when trying to get rawSlickJson. Error:\n" + ex.ToString());
                return null;
            }
#endif
        }
        public void CheckAvailableUpdates(String SUversion, String JsonVersion)
        {
            if (JsonVersion != SUversion)
            {
                MessageBoxResult result = MessageBox.Show("There seems to be a new version of slickupdater available, do you wanna update it it?", "New Update", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Update();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }
        void Update()
        {
            logIt.add("Starting to download new update.");
            WebClient client = new WebClient();
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/updatetemp"))
            {
                Directory.CreateDirectory("updatetemp");
            }
            client.DownloadFile(slickversion.download, "SlickUpdate.zip");
            MessageBox.Show(Directory.GetCurrentDirectory() + @"\updatetemp\");
            SlickUpdater.Unzippy.extract("SlickUpdate.zip", Directory.GetCurrentDirectory() + @"\updatetemp\");
            File.Delete("SlickUpdate.zip");
            Process.Start("cmd", "/C move /Y " + Directory.GetCurrentDirectory() + @"\update\*.* " + Directory.GetCurrentDirectory());
            Environment.Exit(0);
        }
    }
}