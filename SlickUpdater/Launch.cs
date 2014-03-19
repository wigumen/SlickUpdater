using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Controls;

namespace SlickUpdater {
    static class Launch {
        static public void a3Launch(bool connectToServer, string server) {
            string arma3Path = regcheck.arma3RegCheck() + "\\arma3.exe";
            string world = ConfigManager.fetch("ArmA3", "world");
            string customParams = ConfigManager.fetch("ArmA3", "customParameters");
            string mods = modlister();

            string args = "";
            if (ConfigManager.fetch("ArmA3", "window") == "true") {
                args += " -window";
            }
            if (ConfigManager.fetch("ArmA3", "nosplash") == "true") {
                args += " -nosplash";
            }
            if (ConfigManager.fetch("ArmA3", "skipIntro") == "true") {
                args += " -skipIntro";
            }
            if (ConfigManager.fetch("ArmA3", "noLogs") == "true") {
                args += " -noLogs";
            }
            if (ConfigManager.fetch("ArmA3", "noPause") == "true") {
                args += " -noPause";
            }
            if (ConfigManager.fetch("ArmA3", "showScriptErrors") == "true") {
                args += " -showScriptErrors";
            }
            if (connectToServer == true)
            {
                if (server == "PA Repo")
                {
                    args += " -port=2302 -connect=216.155.136.21 -password=PA";
                }
                else if (server == "Test Outfit Repo")
                {
                    args += " -port=2302 -connect=72.5.102.119 -password=scott";
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
        static public void a2Launch(bool connectToServer, string server)
        {
            string arma2Path = regcheck.arma2RegCheck() + "\\ArmA2OA.exe";
            string world = ConfigManager.fetch("ArmA2", "world");
            string customParams = ConfigManager.fetch("ArmA2", "customParameters");
            string mods = modlister();

            string args = "";
            if (ConfigManager.fetch("ArmA2", "window") == "true")
            {
                args += " -window";
            }
            if (ConfigManager.fetch("ArmA2", "nosplash") == "true")
            {
                args += " -nosplash";
            }
            if (ConfigManager.fetch("ArmA2", "skipIntro") == "true")
            {
                args += " -skipIntro";
            }
            if (ConfigManager.fetch("ArmA2", "noLogs") == "true")
            {
                args += " -noLogs";
            }
            if (ConfigManager.fetch("ArmA2", "noPause") == "true")
            {
                args += " -noPause";
            }
            if (ConfigManager.fetch("ArmA2", "showScriptErrors") == "true")
            {
                args += " -showScriptErrors";
            }
            if (connectToServer == true)
            {
                if (server == "PA Repo")
                {
                    // insert ArmA 2 OA server here!!!! !!! 
                    args += " -port=2302 -connect=216.155.136.19 -password=PA";
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
                args += " -mod=" + mods;
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
