using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip;
using System.Windows;
using System.IO;

namespace SlickUpdater {
    static class Unzippy {
        static public void extract(string fileName, string directory) {
            SevenZipExtractor.SetLibraryPath("7z.dll");
            try {
                SevenZipExtractor extractor = new SevenZipExtractor(fileName);
                extractor.Extracting += new EventHandler<ProgressEventArgs>(extr_Extracting);
                extractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(extr_FileExtractionStarted);
                extractor.ExtractionFinished += new EventHandler<EventArgs>(extr_ExtractionFinished);
                extractor.ExtractArchive(directory);
            } 
            catch (System.IO.FileNotFoundException e) {
                MessageBox.Show(e.Message);
            }
        }
        static void extr_Extracting(object sender, EventArgs e) {

        }
        static void extr_FileExtractionStarted(object sender, FileInfoEventArgs e) {
        }
        static void extr_ExtractionFinished(object sender, EventArgs e) {
            
        }

        static private void pluginMove(string modPath) {
            string modPluginDir = modPath + "\\plugin";
            if (Directory.Exists(modPluginDir)) {
                string ts3PluginDir = regcheck.ts3RegCheck() + "\\plugins";
                string[] files = Directory.GetFiles(modPluginDir);
                foreach (string file in files) {
                    string filename = Path.GetFileName(file);
                    try { File.Copy(file, ts3PluginDir + "\\" + filename); } catch (IOException) { };
                }
            }
        }


        static private void directoryCopy(string sourceDirName, string destDirName) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            // Check if source directory exists and return if it doesn't
            if (!dir.Exists) {
                return;
            }

            //List subdirs
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Check if dest directory exists and create it if it doesn't
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }
            
            // Get all files in directory and move each to destination.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files) {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // Recursively (is that the right word?) copy subdirectories and files.
            foreach (DirectoryInfo subdir in dirs) {
                string temppath = Path.Combine(destDirName, subdir.Name);
                directoryCopy(subdir.FullName, temppath);
            }
        }
    }
}
