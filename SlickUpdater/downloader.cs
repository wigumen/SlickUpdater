using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel;
using System.Windows;
using System.IO;

namespace SlickUpdater {
    public static class downloader {
        static bool downloadFinished = true;


        public static void downloadAsync (string url) {
            WebClient client = new WebClient();
            Uri uri = new Uri(url);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            try {
                client.DownloadFileAsync(uri, filename);
            } catch (ArgumentNullException e) {
                MessageBox.Show(e.Message);
            } catch (WebException e) {
                MessageBox.Show(e.Message + " on " + url);
            } catch (InvalidOperationException e) {
                MessageBox.Show(e.Message);
            }

        }
        private static void onComplete(object sender, AsyncCompletedEventArgs args) {
            downloadFinished = true;
        }
        private static void onProgressChanged(object sender, DownloadProgressChangedEventArgs args) {
            WindowManager.mainWindow.worker.ReportProgress(args.ProgressPercentage);
            WindowManager.mainWindow.downloadSpeed = (int)args.BytesReceived;
        }
        public static string webRead(string url) {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();


            return content;
        }

        public static string[] webReadLines(string url) {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);
            StreamReader reader = new StreamReader(stream);
            List<string> list = new List<string>();

            string line;
            while ((line = reader.ReadLine()) != null) {
                list.Add(line);
            }


            return list.ToArray();
        }

        public static string download(string url, WebClient client) {
            downloadFinished = false;
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(onProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(onComplete);
            Uri uri = new Uri(url);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            try { client.DownloadFileAsync(uri, filename); 
            } catch (WebException e) {
                MessageBox.Show(e.Message);
            } catch (NotSupportedException) {
                MessageBox.Show("Tell Slick He Fucked Up!", "A NotSupportedException occurred in the download method");
            }
            while (!downloadFinished) { System.Threading.Thread.Sleep(20);  };
            logIt.addData("Downloaded " + filename);
            return filename;
        }

        public static void folderDelete(string path) {
            DirectoryInfo dir = new DirectoryInfo(path);
            try {
                setAttributesNormal(dir);
                dir.Delete(true);
            } catch (UnauthorizedAccessException e) {
                MessageBox.Show(e.Message);
            } catch (IOException e) {
                MessageBox.Show(e.Message);
            }
            logIt.addData("Deleted directory " + dir);
        }
        static void setAttributesNormal(DirectoryInfo dir) {
            foreach (DirectoryInfo subDirPath in dir.GetDirectories()) {
                setAttributesNormal(subDirPath);
            }
            foreach (FileInfo filePath in dir.GetFiles()) {
                filePath.Attributes = FileAttributes.Normal;
            }
        }
    }
}
