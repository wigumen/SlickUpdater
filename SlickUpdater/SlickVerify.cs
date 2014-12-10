using SlickUpdater;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SlickUpdater
{
    public class SlickVerify
    {
        public void VerifyFiles(String Url, String Dir)
        {
            logit.add("Scanning for mods");

            output("@" + DateTime.Now.ToString("H:mm:ss") + " Starting Scan");

            String[] modlist = WebReadLines(Url + "modlist.cfg");
            string[] tmpa = ModFoldersNotExist(modlist, Dir);
            if (tmpa.Length > 0)
            {
                string tmps = "";
                foreach (string mod in tmpa)
                {
                    tmps += "\n" + mod;
                }
                output("Missing modfolders: " + tmps);
            }
            modlist = ModFoldersExist(modlist, Dir);


            //lists
            List<String> Dirlist = new List<String>();
            List<String> RepoFiles = new List<String>();
            List<String> LocalFiles = new List<String>();
            List<String> DownloadList = new List<String>();
            List<String> HashList = new List<String>();

            //Getting all directories from the modlist
            output("Getting all directories from the modlist:");
            try
            {
                for (int i = 0; i < modlist.Length; i++)
                {
                    String[] dirs = GetDirs(Url + modlist[i] + "/", modlist[i]);
                    for (int u = 0; u < dirs.Length; u++)
                    {
                        
                        Dirlist.Add(dirs[u]);
                    }
                    output("Getting folders currently: " + (i + 1) + "/" + modlist.Length + "(" + modlist[i] + ")");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sector 1\nError:\n" + ex.ToString());
            }

            //Getting all the files which is within the folders
            RepoFiles = GetFiles(Dirlist.ToArray(), Url).ToList();

            //Getting all the local files
            try
            {
                for (int i = 0; i < modlist.Length; i++)
                {
                    LocalFiles = GetLocalFiles(Dir, modlist).ToList();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Sector 2\nError:\n" + ex.ToString());
            }

            //Compare RepoFiles and LocalFiles
            try
            {
                output("Compareing repo hashes and local hashes");
                List<String> LclFiles = LocalFiles;
                for (int i = 0; i < LocalFiles.Count; i++)
                {
                    LclFiles[i] = LclFiles[i].Replace(Dir, "").Replace(@"\", "/").Replace("//", "/");
                }
                for (int i = 0; i < RepoFiles.Count; i++)
                {
                    if (LocalFiles.Contains(RepoFiles[i]) == true)
                    {
                        HashList.Add(RepoFiles[i]);
                        output(RepoFiles[i] + " is OK");
                    }
                    else
                    {
                        DownloadList.Add(RepoFiles[i]);
                        output(RepoFiles[i] + " is MISMATCHED adding to download list");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sector 3\nError:\n" + ex.ToString());
            }

            //Hash files and search for files in need of redownload
            try
            {
                String[] tmp = HashFiles(HashList, Dir, Url);
                foreach (string file in tmp)
                {
                    DownloadList.Add(file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sector 4\nError:\n" + ex.ToString());
            }


            //Writing downloadlist
            try
            {
                List<String> FinalList = new List<String>();
                if (DownloadList.Count > 0)
                {
                    output("Printing downloadlist");

                    for (int i = 0; i < DownloadList.Count; i++)
                    {
                        if (ShouldFileBeIgnored(DownloadList[i]) != true)
                        {
                            if (FinalList.Contains(DownloadList[i]) == false)
                            {
                                FinalList.Add(DownloadList[i]);
                            }

                        }
                    }

                    foreach (string file in FinalList)
                    {
                        addline("Faulty: " + file);
                    }
                }

                logit.add("@" + DateTime.Now.ToString("H:mm:ss") + " Verification done!");
                output("Result: Scanned through " + LocalFiles.Count.ToString() + " files and found " + FinalList.Count.ToString() + " faulty files");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sector 5\nError:\n" + ex.ToString());
            }
        }
        String[] GetDirs(String url, String Mod)
        {
            List<String> list = new List<String>();

            String[] tmp = WebReadLines(url + "dirs.cfg");
            list.Add(Mod);
            if (tmp.Length != 0)
            {
                for (int i = 0; i < tmp.Length; i++)
                {
                    list.Add(Mod + "/" + tmp[i]);
                    Boolean cont = DoesFileWork(url + tmp[i] + "/dirs.cfg");
                    if (cont == true)
                    {
                        string[] tmp2 = GetDirs((url + tmp[i] + "/"), (Mod + "/" + tmp[i]));
                        for (int u = 0; u < tmp2.Length; u++)
                        {
                            list.Add(tmp2[u]);
                        }
                    }
                    else
                    {

                    }
                }
            }

            return list.ToArray();
        }
        String[] GetFiles(String[] dirs, String url)
        {
            List<String> files = new List<String>();

            for (int i = 0; i < dirs.Length; i++)
            {
                string[] tmp = WebReadLines(url + dirs[i] + "/files.cfg");
                for (int u = 0; u < tmp.Length; u++)
                {
                    files.Add(dirs[i] + "/" + tmp[u]);
                }
            }

            return files.ToArray();
        }
        String[] ModFoldersExist(String[] mods, String Dir)
        {
            List<String> returns = new List<String>();
            foreach (String mod in mods)
            {
                if (Directory.Exists(Dir + mod + @"\") == true)
                {
                    returns.Add(mod);
                }
            }
            return returns.ToArray();
        }
        String[] ModFoldersNotExist(String[] mods, String Dir)
        {
            List<String> returns = new List<String>();
            foreach (String mod in mods)
            {
                if (Directory.Exists(Dir + mod + @"\") == false)
                {
                    returns.Add(mod);
                }
            }
            return returns.ToArray();
        }
        String[] GetLocalFiles(String Dir, String[] modlist)
        {
            List<String> LocalFiles = new List<String>();
            for (int i = 0; i < modlist.Length; i++)
            {
                //Collecting all the local files for specific mod in one array
                String[] tmp = Directory.GetFiles(Dir + modlist[i] + @"/", "*.*", System.IO.SearchOption.AllDirectories);

                for (int o = 0; o < tmp.Length; o++)
                {
                    if (ShouldFileBeIgnored(tmp[o]) == false)
                    {
                        LocalFiles.Add(tmp[o]);
                    }
                }
            }
            return LocalFiles.ToArray();
        }
        String[] HashFiles(List<String> HashList, String Dir, String Url)
        {
            List<String> DownloadList = new List<String>();
            for (int i = 0; i < HashList.Count; i++)
            {
                String[] tmp = MD5Compare2((Dir.Replace(@"\", "/") + HashList[i]), (Url + HashList[i]));
                if (tmp[2] == "False")
                {
                    DownloadList.Add(HashList[i]);
                }
            }
            return DownloadList.ToArray();
        }
        static String[] WebReadLines(String url)
        {
            List<String> list = new List<String>();

            HttpWebRequest _Request = (HttpWebRequest)WebRequest.Create(url);
            _Request.Timeout = 3000;
            _Request.ReadWriteTimeout = 3000;
            string line = "";
            try
            {
                WebResponse _Response = _Request.GetResponse();
                Stream _Stream = _Response.GetResponseStream();

                StreamReader _Reader = new StreamReader(_Stream);
                while ((line = _Reader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return list.ToArray();
        }
        Boolean DoesFileWork(String url)
        {
            Boolean doeswork = false;

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();

            if (String.IsNullOrEmpty(content) == true)
            {
                doeswork = false;
            }
            else
            {
                doeswork = true;
            }

            return doeswork;
        }
        Boolean ShouldFileBeIgnored(String name)
        {
            String[] ignorelist = { "zsync" };
            if (name == "SU.version")
            {
                return true;
            }
            else
            {
                String[] tmparray = name.Split('.').ToArray();
                foreach (String ignore in ignorelist)
                {
                    for (int i = 0; i < tmparray.Length; i++)
                    {
                        if (tmparray[i] == ignore)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        String[] MD5Compare(String file, String url)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    string same = null;
                    var rawhash = md5.ComputeHash(File.ReadAllBytes(file));
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < rawhash.Length; i++)
                    {
                        sb.Append(rawhash[i].ToString("X2").ToLower());
                    }

                    WebClient WClient = new WebClient();
                    Stream Stream = WClient.OpenRead(url + ".hash");
                    StreamReader Reader = new StreamReader(Stream);
                    String Hashfromnet = Reader.ReadToEnd();

                    if (sb.ToString() == Hashfromnet)
                    {
                        same = "True";
                    }
                    else
                    {
                        same = "False";
                    }
                    string[] returns = { sb.ToString(), Hashfromnet, same };
                    return returns;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return MD5Compare(file, url);
                output("Error while comparing hashes, rerunning");
            }
        }
        String[] MD5Compare2(String file, String url)
        {
            string same = null;
            StringBuilder sb = new StringBuilder();
            FileStream fs = new FileStream(file, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(fs);
            fs.Close();

            foreach (byte hex in hash)
            {
                sb.Append(hex.ToString("X2").ToLower());
            }

            WebClient WClient = new WebClient();
            Stream Stream = WClient.OpenRead(url + ".hash");
            StreamReader Reader = new StreamReader(Stream);
            String Hashfromnet = Reader.ReadToEnd();

            if (sb.ToString() == Hashfromnet)
            {
                same = "True";
            }
            else
            {
                same = "False";
            }

            string[] returns = { sb.ToString(), Hashfromnet, same };
            return returns;
        }

        void output(String text)
        {
            //MainWindow.Instance.lbl_cuop.Content = text;
            //MainWindow.Instance.lview_mods.Items.Add(text);
            logit.add(text);
        }

        void addline(string text)
        {
            //MainWindow.Instance.lview_mods.Items.Add(text);
            logit.add(text);
        }
    }
    public class SlickJson
    {
        public String Version { get; set; }
        public String Downloadurl { get; set; }
        public List<RepoFormat> repos { get; set; }
    }
    public class RepoFormat
    {
        public String Name { get; set; }
        public String Url { get; set; }
        public String Server { get; set; }
    }
}
