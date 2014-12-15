using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SlickUpdater {
    static class Launch {
        static public void a3Launch(bool connectToServer, string server, string password) {
            var arma3Path = regcheck.arma3RegCheck() + "\\arma3.exe";
            var world = Properties.Settings.Default.world;
            var customParams = Properties.Settings.Default.customParams;
            var mods = Modlister();

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
                args += " " + cparams();
            }
            if (mods != "") {
                args += " -mod=\"" + mods + cParamsMods() + "\"";
            }
            
            Process.Start(arma3Path, args);
            logIt.add("Launched Arma 3 with " + args);
        }
        //added ArmA2 OA launcher
        static public void a2Launch(bool connectToServer, string server, string password)
        {
            string arma2Path = regcheck.arma2RegCheck() + "\\ArmA2OA.exe";
            string varma2Path = regcheck.varma2RegCheck();
            string world = Properties.Settings.Default.world;
            string customParams = Properties.Settings.Default.customParams;
            string mods = Modlister();
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
            logIt.add("Launched Arma 2 with " + args);
        }
        static private string Modlister() {
            string modlist = "";
            string Path = path();

            foreach (Mod item in WindowManager.mainWindow.a3ModList.Items) {
                if (modlist == "") {
                    modlist = Path + item.modName + ";";
                } else {
                    modlist += Path + item.modName + ";";
                }
            }
            return modlist;
        }
        static private string cParamsMods()
        {
            string result = "";
            string Path = path();
            Regex rgex = new Regex("(@[A-z0-9_\\:]*|C:\\@[A-z0-9_\\:]*)");
            MatchCollection coll = rgex.Matches(Properties.Settings.Default.customParams);

            for (int i = 0; i < coll.Count; i++)
            {
                result += Path + coll[i] + ";";
            }

            return result;
        }
        static private string cparams()
        {
            string tmp = Properties.Settings.Default.customParams;
            Regex rgex = new Regex("(@[A-z0-9_\\:]*|C:\\@[A-z0-9_\\:]*)");
            MatchCollection coll = rgex.Matches(tmp);

            for (int i = 0; i < coll.Count; i++)
            {
                tmp = tmp.Replace(coll[i].ToString(), "");
            }

            tmp = tmp.Replace("-mod=", "").Replace(";", "");

            return tmp;
        }
        static string path()
        {
            string path = "";
            if (Properties.Settings.Default.gameversion == "ArmA3")
            {
                if (Properties.Settings.Default.A3path != Properties.Settings.Default.ModPathA3)
                {
                    path = Properties.Settings.Default.ModPathA3 + @"\";
                }
            }
            if (Properties.Settings.Default.gameversion == "ArmA2")
            {
                if (Properties.Settings.Default.A2path != Properties.Settings.Default.ModPathA3)
                {
                    path = Properties.Settings.Default.ModPathA2 + @"\";
                }
            }
            return path;
        }
    }
}
