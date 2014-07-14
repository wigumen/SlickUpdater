using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Controls;

namespace SlickUpdater {
    static class Launch {
        static public void a3Launch(bool connectToServer, string server, string password) {
            string arma3Path = regcheck.arma3RegCheck() + "\\arma3.exe";
            string world = Properties.Settings.Default.world;
            string customParams = Properties.Settings.Default.customParams;
            string mods = modlister();

            string args = "";
            if (Properties.Settings.Default.window == true) {
                args += " -window";
            }
            if (Properties.Settings.Default.nosplash == true) {
                args += " -nosplash";
            }
            if (Properties.Settings.Default.skipIntro == true) {
                args += " -skipIntro";
            }
            if (Properties.Settings.Default.noLogs == true) {
                args += " -noLogs";
            }
            if (Properties.Settings.Default.noPause == true) {
                args += " -noPause";
            }
            if (Properties.Settings.Default.showScriptErrors == true) {
                args += " -showScriptErrors";
            }
            if (connectToServer == true)
            {
                if (server != null)
                {
                    if (password != null)
                    {
                        args += " -port=2302 -connect=" + server + " -password=" + password;
                    }
                    else
                    {
                        args += " -port=2302 -connect=" + server;
                    }
                }
            }
            if (world != "") {
                if (world == "demwaffels") {
                    System.Diagnostics.Process.Start("http://www.youtube.com/watch?v=8W5WdS7q3ns");
                    return;
                } else {
                    args += " -world=" + world;
                }
            }
            if (customParams != "") {
                args += " " + customParams;
            }
            if (mods != "") {
                args += " -mod=" + mods;
            }

            Process.Start(arma3Path, args);
            logIt.addData("Launched Arma 3 with " + args);
        }
        //added ArmA2 OA launcher
        static public void a2Launch(bool connectToServer, string server, string password)
        {
            string arma2Path = regcheck.arma2RegCheck() + "\\ArmA2OA.exe";
            string varma2Path = regcheck.varma2RegCheck();
            string world = Properties.Settings.Default.world;
            string customParams = Properties.Settings.Default.customParams;
            string mods = modlister();
            string args = "";

            if (Properties.Settings.Default.window == true)
            {
                args += " -window";
            }
            if (Properties.Settings.Default.nosplash == true)
            {
                args += " -nosplash";
            }
            if (Properties.Settings.Default.skipIntro == true)
            {
                args += " -skipIntro";
            }
            if (Properties.Settings.Default.noLogs == true)
            {
                args += " -noLogs";
            }
            if (Properties.Settings.Default.noPause == true)
            {
                args += " -noPause";
            }
            if (Properties.Settings.Default.showScriptErrors == true)
            {
                args += " -showScriptErrors";
            }
            if (connectToServer == true)
            {
                if (server != null)
                {
                    if (password != null)
                    {
                        args += " -port=2302 -connect=" + server + " -password=" + password;
                    }
                    else
                    {
                        args += " -port=2302 -connect=" + server;
                    }
                }
            }
            if (world != "")
            {
                if (world == "demwaffels")
                {
                    System.Diagnostics.Process.Start("http://www.youtube.com/watch?v=8W5WdS7q3ns");
                    return;
                }
                else
                {
                    args += " -world=" + world;
                }
            }
            if (customParams != "")
            {
                args += " " + customParams;
            }
            if (mods != "")
            {
                args += " \"-mod=" + varma2Path + ";EXPANSION;ca\"" + " \"-beta=Expansion\\beta;Expansion\\beta\\Expansion\" " + " \"-mod=" + mods + "\"";
            }

            Process.Start(arma2Path, args);
            logIt.addData("Launched Arma 2 with " + args);
        }
        static private string modlister() {
            string modlist = "";
            foreach (Mod item in WindowManager.mainWindow.a3ModList.Items) {
                if (modlist == "") {
                    modlist = item.modName + ";";
                } else {
                    modlist += item.modName + ";";
                }
            }
            return modlist;
        }
    }
}
