using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Media.Imaging;
using SlickUpdater.Properties;
using System.Diagnostics;

namespace SlickUpdater
{
    public static class UpdateManager
    {
        public static string armaPath = Settings.Default.A3path;
        private static Queue<string> queue = new Queue<string>();
        public static bool isUpdateStarted;
        private static string url = "http://arma.projectawesome.net/beta/repo";
        private static string modlist = "modlist.cfg";
        public static bool a3UpdateComplete;
        private static int updateProgress;
        private static int totalFiles;
        public static bool isArma2 = false;
        public static Boolean TFRalert = false;


        public static void UpdateCheck()
        {
            //Boolean TFRalert = false;
            if (isArma2)
            {
                armaPath = path("ArmA2");
            }
            else
            {
                armaPath = path("ArmA3");
            }
            string mod;
            int index;
            string[] mods;
            string modFolder;
            string versionFile;
            string versionString;
            string version0String;
            string a3Repourl = Settings.Default.A3repourl;
            versionfile slickversion = WindowManager.mainWindow.Slickversion;
            //string slickVersion = downloader.webRead("http://projectawesomemodhost.com/beta/repo/slickupdater/slickversion");
            /*
#if DEBUG
            xmlLine = "http://localhost/repo/";
#endif
             */
            //string[] parsedslickVersion = slickVersion.Split('%');
            if (a3Repourl != "")
            {
                url = a3Repourl;
            }
            else
            {
                MessageBox.Show("Your repourl is not set. Go into settings and change it! Setting it to default!");
                url = slickversion.repos[0].url;
                Settings.Default.A3repourl = slickversion.repos[0].url;
            }


            var modRed =
                new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modRed.png"));
            var modGreen =
                new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modGreen.png"));
            var modBlue =
                new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modBlue.png"));
            var modBrown =
                new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modBrown.png"));
            var modYellow =
                new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modYellow.png"));
            var a3Items = new List<Mod>();
            try
            {
                mods = downloader.webReadLines(url + modlist);
            }
            catch (WebException e)
            {
                WindowManager.mainWindow.CheckWorker.ReportProgress(-1, e.Message);
                return;
            }
            bool date = true;
            foreach (string modline in mods)
            {
                index = modline.IndexOf("#");
                if (index != 0)
                {
                    if (index != -1)
                    {
                        mod = modline.Substring(0, index);
                    }
                    else
                    {
                        mod = modline;
                    }
                    modFolder = armaPath + "\\" + mod;
                    versionFile = armaPath + "\\" + mod + "\\SU.version";
                    version0String = downloader.webRead(url + "/" + mod + "/" + "SU.version"); 
                    if (Directory.Exists(modFolder))
                    {
                        if (File.Exists(versionFile))
                        {
                            versionString = File.ReadAllText(versionFile);
                            if (versionString == version0String)
                            {
                                modGreen.Freeze();
                                a3Items.Add(new Mod
                                {
                                    status = modGreen,
                                    modName = mod,
                                    version = "v. " + versionString,
                                    servVersion = "v. " + version0String
                                });
                                logIt.add(mod + " is up to date.");
                                //MessageBox.Show(mod + " is up to date.");
                            }
                            else
                            {
                                modYellow.Freeze();
                                a3Items.Add(new Mod
                                {
                                    status = modYellow,
                                    modName = mod,
                                    version = "v. " + versionString,
                                    servVersion = "v. " + version0String
                                });
                                date = false;
                                //MessageBox.Show(mod + " is out of date.");
                                logIt.add(mod + " is out to date.");
                                if (mod == "@task_force_radio")
                                {
                                    TFRalert = true;
                                }
                            }
                        }
                        else
                        {
                            modBrown.Freeze();
                            a3Items.Add(new Mod
                            {
                                status = modBrown,
                                modName = mod,
                                version = "No file",
                                servVersion = "v. " + version0String
                            });
                            date = false;
                            //MessageBox.Show(mod + " is missing a version file.");
                            logIt.add(mod + " is missing a version file.");
                        }
                    }
                    else
                    {
                        modBlue.Freeze();
                        a3Items.Add(new Mod
                        {
                            status = modBlue,
                            modName = mod,
                            version = "No file",
                            servVersion = "v. " + version0String
                        });
                        //File.Delete(versionFile);
                        date = false;
                        //MessageBox.Show(mod + " doesn't exist on your computer.");
                        logIt.add(mod + " doesn't exist on your computer.");
                        if (mod == "@task_force_radio")
                        {
                            TFRalert = true;
                        }
                    }
                }
            }
            if (date)
            {
                WindowManager.mainWindow.CheckWorker.ReportProgress(1, "Launch " + WindowManager.mainWindow.CurrentGame);
            }
            else
            {
                WindowManager.mainWindow.CheckWorker.ReportProgress(1, "Update " + WindowManager.mainWindow.CurrentGame);
            }
            WindowManager.mainWindow.CheckWorker.ReportProgress(2, a3Items);
        }

        public static void a3Update()
        {
            if (isArma2)
            {
                armaPath = path("ArmA2");
            }
            else
            {
                armaPath = path("ArmA3");
            }
            if (url == "")
            {
                url = Settings.Default.A3repourl;
            }
            if (isArma2)
            {
                string arma3Path = regcheck.arma2RegCheck();
            }
            else
            {
                string arma3Path = regcheck.arma3RegCheck();
            }
            string mod;
            string[] mods;
            string modFolder;
            string versionString = "";
            string version0String = "";
            var client = new WebClient();

            mods = downloader.webReadLines(url + modlist);
            int i = 0;
            foreach (string modline in mods)
            {
                i++;
                int index = modline.IndexOf("#");
                if (index != 0)
                {
                    if (index != -1)
                    {
                        mod = modline.Substring(0, index);
                    }
                    else
                    {
                        mod = modline;
                    }
                    modFolder = armaPath + "\\" + mod;
                    if (Directory.Exists(modFolder))
                    {
                        string versionFile = armaPath + "\\" + mod + "\\" + "SU.version";
                        string version0File = "SU.version";
                        if (File.Exists(versionFile))
                        {
                            versionString = File.ReadAllText(versionFile);
                            version0String = downloader.webRead(url + mod + "\\" + version0File);
                            logIt.add("Fetched versionfile from server version is " + versionString);
                            File.Delete(version0File);
                            if (versionString == version0String)
                            {
                                //a3Items.Add(new Mod() { status = modGreen, modName = mod });
                                //MessageBox.Show(mod + " is up to date.");
                            }
                            else
                            {
                                //a3Items.Add(new Mod() { status = modYellow, modName = mod });
                                //MessageBox.Show(mod + " is out of date.");
                                a3DetailUpdate(mod, client);
                            }
                        }
                        else
                        {
                            //a3Items.Add(new Mod() { status = modBrown, modName = mod });
                            //MessageBox.Show(mod + " is missing a version file.");
                            version0String = downloader.webRead(url + mod + "\\" + version0File);
                            MessageBoxResult result =
                                MessageBox.Show(
                                    "SlickUpdater have detected that you have the folder " + modFolder +
                                    " if your 100% sure this is up to date you don't have to re-download. \n\nAre you sure this mod is up to date?",
                                    "Mod folder detacted", MessageBoxButton.YesNo);
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    File.WriteAllText(modFolder + "\\SU.version", version0String);
                                    break;
                                case MessageBoxResult.No:
                                    a3DetailUpdate(mod, client);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //a3Items.Add(new Mod() { status = modBlue, modName = mod });
                        //MessageBox.Show(mod + " doesn't exist on your computer.");
                        a3DetailUpdate(mod, client);
                    }
                }
                double status = i/(double) mods.Length;
                WindowManager.mainWindow.Worker.ReportProgress((int) (status*100) + 202);
            }
        }

        private static void increment()
        {
            double progress = (++updateProgress/(double) totalFiles);
            WindowManager.mainWindow.Worker.ReportProgress((int) (progress*100) + 101);
        }

        private static void a3DetailUpdate(string mod, WebClient client)
        {
            string arma3Path = "";
            if (isArma2)
            {
                //arma3Path = regcheck.arma2RegCheck();
                arma3Path = path("ArmA2");
            }
            else
            {
                //arma3Path = regcheck.arma3RegCheck();
                arma3Path = path("ArmA3");
            }

            string modPath = arma3Path + "\\" + mod;

            Directory.CreateDirectory(arma3Path + "\\" + mod);

            updateProgress = 0;
            try
            {
                totalFiles = Convert.ToInt32(downloader.webRead(url + mod + "/count.txt"));
            }
            catch (WebException)
            {
                WindowManager.mainWindow.Worker.ReportProgress(-1,
                    "Web exception in reading " + url + mod + "/count.txt");
                return;
            }
            catch (Exception e)
            {
                WindowManager.mainWindow.Worker.ReportProgress(-1, e.Message);
                return;
            }
            checkFilesFolders(arma3Path + "\\" + mod);

            downloader.download(url + mod + "/SU.version", client);
            File.Delete(arma3Path + "\\" + mod + "\\SU.version");
            File.Move("SU.version", arma3Path + "\\" + mod + "\\SU.version");
        }

        private static void checkFilesFolders(string folder)
        {
            if (isArma2)
            {
                //armaPath = regcheck.arma2RegCheck();
                armaPath = path("ArmA2");
            }
            else
            {
                //armaPath = Settings.Default.A3path;
                armaPath = path("ArmA3");
            }
            string relativePath = folder.Replace(armaPath, "");
            string[] files = downloader.webReadLines(url + relativePath.Replace(@"\\", "") + "/files.cfg");

            var info = new DirectoryInfo(folder);

            string[] dirs = downloader.webReadLines(url + relativePath + "\\dirs.cfg");

            foreach (DirectoryInfo dirInfo in info.GetDirectories())
            {
                bool exists = false;
                foreach (string dir in dirs)
                {
                    if (dir == dirInfo.Name)
                    {
                        exists = true;
                    }
                }
                if (!exists)
                {
                    dirInfo.Delete(true);
                }
            }

            foreach (string dir in dirs)
            {
                var dirInfo = new DirectoryInfo(folder + "\\" + dir);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                checkFilesFolders(dirInfo.FullName);
            }

            foreach (FileInfo file in info.GetFiles())
            {
                bool exists = false;
                foreach (string fileString in files)
                {
                    if (file.Name == fileString)
                    {
                        exists = true;
                    }
                }
                if (exists == false)
                {
                    file.Delete();
                }
            }
            var client = new WebClient();
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(folder + "\\" + file);
                if (fileInfo.Exists)
                {
                    string hash = RepoGenerator.md5Calc(fileInfo.FullName);
                    string downloadedHash = downloader.webRead(url + relativePath + "\\" + fileInfo.Name + ".hash");
                    if (hash != downloadedHash)
                    {
                        downloader.download(url + relativePath + "\\" + fileInfo.Name + ".7z", client);
                        Unzippy.extract(fileInfo.Name + ".7z", fileInfo.DirectoryName);
                        increment();
                        File.Delete(fileInfo.Name + ".7z");
                    }
                }
                else
                {
                    downloader.download(url + relativePath + "\\" + fileInfo.Name + ".7z", client);
                    Unzippy.extract(fileInfo.Name + ".7z", fileInfo.DirectoryName);
                    increment();
                    File.Delete(fileInfo.Name + ".7z");
                }
            }

            if (info.Name == "plugin")
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(info.FullName, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins"));

                //Copy all the files
                foreach (string newPath in Directory.GetFiles(info.FullName, "*.*", SearchOption.AllDirectories))
                {
                    if (!File.Exists(newPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins")))
                    {
                        try
                        {
                            File.Copy(newPath, newPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins"),
                                true);
                            logIt.add("Copied ACRE plugin to TS3 folder");
                        }
                        catch (Exception e)
                        {
                            WindowManager.mainWindow.Worker.ReportProgress(-1, e.Message);
                            logIt.add("Failed to copy ACRE plugin to TS3 folder. Error Message: " + e.Message);
                        }
                    }
                    else
                    {
                        Process[] pro64 = Process.GetProcessesByName("ts3client_win64");
                        Process[] pro32 = Process.GetProcessesByName("ts3client_win32");

                        if (pro32.Length == 0 && pro64.Length == 0)
                        {
                            logIt.add("TS3 is not running");
                            File.Delete(newPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins"));
                            File.Copy(newPath, newPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins"));
                        }
                        else
                        {
                            logIt.add("TS3 is running");
                            if (UpdateManager.TFRalert == true)
                            {
                                MessageBox.Show("Teamspeak will now be closed to update the plugin files.", "Teamspeak will now close...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                            foreach(Process p in pro64)
                            {
                                p.Kill();
                            }

                            foreach(Process p in pro32)
                            {
                                p.Kill();
                            }

                            File.Delete(newPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins"));
                            File.Copy(newPath, newPath.Replace(info.FullName, Settings.Default.ts3Dir + "\\plugins"));
                        }
                    }
                }
            }

            if (info.Name == "userconfig")
            {
                string output = armaPath + "\\userconfig";
                Directory.CreateDirectory(output);

                foreach (string dirPath in Directory.GetDirectories(info.FullName, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(info.FullName, output));

                foreach (string newPath in Directory.GetFiles(info.FullName, "*.*",
                    SearchOption.AllDirectories))
                    try
                    {
                        File.Copy(newPath, newPath.Replace(info.FullName, output), true);
                    }
                    catch (Exception e)
                    {
                        WindowManager.mainWindow.Worker.ReportProgress(-1, e.Message);
                    }
            }
        }

        public static void downloadAsync(string url)
        {
            var client = new WebClient();
            var uri = new Uri(url);
            string filename = Path.GetFileName(uri.LocalPath);
            try
            {
                client.DownloadFileAsync(uri, filename);
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show(e.Message);
                logIt.add(e.Message);
            }
            catch (WebException e)
            {
                MessageBox.Show(e.Message + " on " + url);
                logIt.add(e.Message + " on " + url);
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.Message);
                logIt.add(e.Message);
            }
        }
        static string path(string game)
        {
            string path = "";
            if (game == "ArmA3")
            {
                if (Properties.Settings.Default.A3path != Properties.Settings.Default.ModPathA3)
                {
                    path = Properties.Settings.Default.ModPathA3;
                }
                else
                {
                    path = Settings.Default.A3path;
                }
            }
            if (game == "ArmA2")
            {
                if (Properties.Settings.Default.A2path != Properties.Settings.Default.ModPathA3)
                {
                    path = Properties.Settings.Default.ModPathA2;
                }
                else
                {
                    path = Settings.Default.A2path;
                }
            }
            return path;
        }
    }
}