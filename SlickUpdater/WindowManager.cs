using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlickUpdater {
    public static class WindowManager {
        internal static MainWindow mainWindow = null;
        internal static RepoGen_Options repoGen_Options = null;

        public static void repoGen_Options_setWnd(RepoGen_Options wnd) {
            repoGen_Options = wnd;
        }

        public static void SetWnd(MainWindow wnd) {
            mainWindow = wnd;
        } 
    }
}
