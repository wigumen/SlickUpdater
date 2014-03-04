using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenZip;

namespace SlickUpdater {
    static class Zippy {
        static public void compress(string source, string outputFileName) {
            if (source != null && outputFileName != null) {
                SevenZipCompressor.SetLibraryPath("7z.dll");
                SevenZipCompressor cmp = new SevenZipCompressor();
                cmp.Compressing += new EventHandler<ProgressEventArgs>(cmp_Compressing);
                cmp.FileCompressionStarted += new EventHandler<FileNameEventArgs>(cmp_FileCompressionStarted);
                cmp.CompressionFinished += new EventHandler<EventArgs>(cmp_CompressionFinished);
                cmp.ArchiveFormat = (OutArchiveFormat)Enum.Parse(typeof(OutArchiveFormat), "SevenZip");
                cmp.CompressFiles(outputFileName, source);
            }
        }
        static private void cmp_Compressing(object sender, ProgressEventArgs e) {

        }
        static private void cmp_FileCompressionStarted(object sender, FileNameEventArgs e) {

        }
        static private void cmp_CompressionFinished(object sender, EventArgs e) {
        }
    }
}
