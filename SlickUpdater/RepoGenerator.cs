using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Threading;

namespace SlickUpdater {
    static class RepoGenerator {

        private static Queue<string[]> queue = new Queue<string[]>();
        static BackgroundWorker bw1 = new BackgroundWorker();

        public static void inputGen() {
            //string dirPath = ConfigManager.fetch("repoGen", "inputDir");
            string dirPath = Properties.Settings.Default.inputDir;
            if (dirPath != "") {

                BitmapImage modRed = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modRed.png"));
                BitmapImage modGreen = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modGreen.png"));
                List<modSourceFolder> modSource = new List<modSourceFolder>();

                DirectoryInfo dir = new DirectoryInfo(dirPath);
                DirectoryInfo[] tempSubDir = dir.GetDirectories();

                foreach (DirectoryInfo i in tempSubDir) {
                    string fullName = i.FullName;
                    Uri uri = new Uri(fullName);
                    string name = Path.GetFileName(uri.LocalPath);
                    if (name.Substring(0, 1) == "@") {
                        string version = "1";
                        string versionFile = fullName + "\\SU.version";
                        if (File.Exists(versionFile)) {
                            string[] linesRead = File.ReadAllLines(versionFile);
                            version = linesRead[0];
                        }
                        modSource.Add(new modSourceFolder() { modFolderName = name, modVersion = version });
                    }
                }
                List<modSourceFolder> outputSource = new List<modSourceFolder>();
                WindowManager.mainWindow.outputDirListBox.ItemsSource = outputSource;
                WindowManager.mainWindow.inputDirListBox.ItemsSource = modSource;
            }
        }

        public static void startGen() {
            bw1.RunWorkerAsync();
            bw1.DoWork += generate;
            bw1.RunWorkerCompleted += bw_Completed;
        }

        private static void bw_Completed(object sender, RunWorkerCompletedEventArgs e) {
            bw1.RunWorkerCompleted -= bw_Completed;
            WindowManager.mainWindow.IsEnabled = true;
        }

        private static void generate(object sender, DoWorkEventArgs e) {
            bw1.DoWork -= generate;
            BitmapImage modRed = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modRed.png"));
            BitmapImage modGreen = new BitmapImage(new Uri(@"pack://application:,,,/Slick Updater Beta;component/Resources/modGreen.png"));

            DataGrid listView = WindowManager.mainWindow.outputDirListBox;

            string outputDir = Properties.Settings.Default.outputDir;
            DirectoryInfo outputDirInfo = new DirectoryInfo(outputDir);

            if (!Directory.Exists(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }

            outputDirInfo.Delete(true);



            Directory.CreateDirectory(outputDir);
            using (StreamWriter modsw = new StreamWriter(outputDir + "\\modlist.cfg", true)) {
                for (int i = 0; i < listView.Items.Count; i++) {
                    modSourceFolder modSource = listView.Items.GetItemAt(i) as modSourceFolder;
                    string modFolderName = modSource.modFolderName;
                    string modVersionNumber = modSource.modVersion;
                    string inputDir = Properties.Settings.Default.inputDir;
                    if (Directory.Exists(inputDir)) {
                        string inputModDir = inputDir + "\\" + modFolderName;
                        //genFile(inputModDir);
                        DirectoryInfo dir = new DirectoryInfo(inputModDir);
                        if (dir.Exists) {

                            modsw.WriteLine(modFolderName);
                            //Create output Directory if it doesn't exist.
                            //Create modfolder in outputdir if it doesn't exits
                            if (!Directory.Exists(outputDir + "\\" + modFolderName)) {
                                Directory.CreateDirectory(outputDir + "\\" + modFolderName);
                            }
                            string fileCfgPath = outputDir + "\\" + modFolderName + "\\files.cfg";
                            using (StreamWriter sw = new StreamWriter(fileCfgPath, true)) {
                                foreach (FileInfo fileInfo in dir.GetFiles()) {
                                    if (fileInfo.Name != "SU.version") {
                                        string name = fileInfo.Name;
                                        string path = fileInfo.FullName;
                                        string zipName = name + ".7z";
                                        string zipPath = outputDir + "\\" + modFolderName + "\\" + zipName;
                                        string hashPath = outputDir + "\\" + modFolderName + "\\" + name + ".hash";
                                        File.WriteAllText(hashPath, md5Calc(path));
                                        //string[] temp = {path, zipPath};
                                        //queue.Enqueue(temp);
                                        Zippy.compress(path, zipPath);
                                        sw.WriteLine(name);
                                    }
                                }
                            }
                            string dirCfgPath = outputDir + "\\" + modFolderName + "\\dirs.cfg";
                            using (StreamWriter sw = new StreamWriter(dirCfgPath, true)) {
                                foreach (DirectoryInfo directory in dir.GetDirectories()) {
                                    string sourcePath = directory.FullName;
                                    string sourceName = directory.Name;
                                    string outDir = outputDir + "\\" + modFolderName + "\\" + sourceName;
                                    genFile(sourcePath, outDir);
                                    genDir(sourcePath, outDir);
                                    sw.WriteLine(sourceName);
                                }
                            }
                            File.WriteAllText(outputDir + "\\" + modFolderName + "\\SU.version", modVersionNumber);
                            DirectoryInfo source = new DirectoryInfo(inputDir + "\\" + modFolderName);
                            FileInfo[] sourceFiles = source.GetFiles("*", SearchOption.AllDirectories);
                            File.WriteAllText(outputDir + "\\" + modFolderName + "\\count.txt", sourceFiles.Length.ToString());
                        }
                    }
                }
            }
        }

        private static void genDir(string inputDir, string outputDir) {
            string dirCfgPath = outputDir + "\\dirs.cfg";
            using (StreamWriter sw = new StreamWriter(dirCfgPath, true)) {
                DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
                foreach (DirectoryInfo dir in inputDirInfo.GetDirectories()) {
                    string path = dir.FullName;
                    string name = dir.Name;
                    if (!Directory.Exists(outputDir + "\\" + name)) {
                        Directory.CreateDirectory(outputDir + "\\" + name);
                    }
                    genFile(path, outputDir + "\\" + name);
                    genDir(path, outputDir + "\\" + name);
                    sw.WriteLine(dir.Name);
                }
            }
        }

        private static void genFile(string inputDir, string outputDir) {
            DirectoryInfo inputDirInfo = new DirectoryInfo(inputDir);
            DirectoryInfo outputDirInfo = new DirectoryInfo(outputDir);

            if (!outputDirInfo.Exists) {
                outputDirInfo.Create();
            }

            string fileCfgPath = outputDir + "\\files.cfg";
            using (StreamWriter sw = new StreamWriter(fileCfgPath, true)) {
                foreach (FileInfo file in inputDirInfo.GetFiles()) {
                    string name = file.Name;
                    string path = file.FullName;
                    string zipName = name + ".7z";
                    string zipPath = outputDir + "\\" + zipName;
                    string hashPath = outputDir + "\\" + name + ".hash";
                    File.WriteAllText(hashPath, md5Calc(path));
                    Zippy.compress(path, zipPath);
                    //string[] temp = {path, zipPath};
                    //queue.Enqueue(temp);
                    sw.WriteLine(name);
                }
            }
        }
        public static string md5Calc(string file) {
            MD5 md5 = MD5.Create();
            byte[] buffer;
            try {
                FileStream stream = File.OpenRead(file);
                try {
                    buffer = md5.ComputeHash(stream);
                } catch (FileNotFoundException) {
                    return "FileNotFound";
                } finally {
                    stream.Close();
                }
            } catch {
                buffer = null;
            } 
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++) {
                sb.Append(buffer[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static void deQueue() {
            if (queue.Any()) {
                string[] temp = queue.Dequeue();
                Zippy.compress(temp[0], temp[1]);
            } else {
                WindowManager.mainWindow.IsEnabled = true;
            }
        }
    }

    public class modSourceFolder {
        public string modFolderName { get; set; }

        public string modVersion { get; set; }

        //public ImageSource status { get ; set; } 
    }
}
