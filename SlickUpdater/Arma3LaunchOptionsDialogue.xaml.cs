using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SlickUpdater {
    /// <summary>
    /// Interaction logic for Arma3LaunchOptionsDialogue.xaml
    /// </summary>

    public partial class Arma3LaunchOptionsDialogue : Window {
        public static Arma3LaunchOptionsDialogue Instance;
        public Arma3LaunchOptionsDialogue() {
            InitializeComponent();
            initializeValues();
            Instance = this;
        }
        void initializeValues() {

            if (Properties.Settings.Default.window == true) {
                window.IsChecked = true;
            } else {
                window.IsChecked = false;
            }

            if (Properties.Settings.Default.nosplash == true) {
                nosplash.IsChecked = true;
            } else {
                nosplash.IsChecked = false;
            }

            if (Properties.Settings.Default.skipIntro == true) {
                skipIntro.IsChecked = true;
            } else {
                skipIntro.IsChecked = false;
            }

            if (Properties.Settings.Default.noLogs == true) {
                noLogs.IsChecked = true;
            } else {
                noLogs.IsChecked = false;
            }

            if (Properties.Settings.Default.noPause == true) {
                noPause.IsChecked = true;
            } else {
                noPause.IsChecked = false;
            }

            if (Properties.Settings.Default.showScriptErrors == true) {
                showScriptErrors.IsChecked = true;
            } else {
                showScriptErrors.IsChecked = false;
            }

            world.Text = Properties.Settings.Default.world;
            customParams.Text = Properties.Settings.Default.customParams;
        }

        private void window_Click(object sender, RoutedEventArgs e) {
            if (window.IsChecked == true) {
                Properties.Settings.Default.window = true;
                //ConfigManager.write("ArmA3", "window", "true");
            } else {
                //ConfigManager.write("ArmA3", "window", "false");
                Properties.Settings.Default.window = false;
            }
        }

        private void nosplash_Click(object sender, RoutedEventArgs e) {
            if (nosplash.IsChecked == true) {
                Properties.Settings.Default.nosplash = true;
                //ConfigManager.write("ArmA3", "nosplash", "true");
            } else {
                Properties.Settings.Default.nosplash = false;
                //ConfigManager.write("ArmA3", "nosplash", "false");
            }
        }

        private void skipIntro_Click(object sender, RoutedEventArgs e) {
            if (skipIntro.IsChecked == true) {
                Properties.Settings.Default.skipIntro = true;
                //ConfigManager.write("ArmA3", "skipIntro", "true");
            } else {
                Properties.Settings.Default.skipIntro = false;
                //ConfigManager.write("ArmA3", "skipIntro", "false");
            }
        }

        private void noLogs_Click(object sender, RoutedEventArgs e) {
            if (noLogs.IsChecked == true) {
                Properties.Settings.Default.noLogs = true;
                //ConfigManager.write("ArmA3", "noLogs", "true");
            } else {
                Properties.Settings.Default.noLogs = false;
                //ConfigManager.write("ArmA3", "noLogs", "false");
            }
        }

        private void noPause_Click(object sender, RoutedEventArgs e) {
            if (noPause.IsChecked == true) {
                Properties.Settings.Default.noPause = true;
                //ConfigManager.write("ArmA3", "noPause", "true");
            } else {
                Properties.Settings.Default.noPause = false;
                //ConfigManager.write("ArmA3", "noPause", "false");
            }
        }

        private void showScriptErrors_Click(object sender, RoutedEventArgs e) {
            if (showScriptErrors.IsChecked == true) {
                Properties.Settings.Default.showScriptErrors = true;
                //ConfigManager.write("ArmA3", "showScriptErrors", "true");
            } else {
                Properties.Settings.Default.showScriptErrors = false;
                //ConfigManager.write("ArmA3", "showScriptErrors", "false");
            }
        }

        private void world_TextChanged(object sender, TextChangedEventArgs e) {
            Properties.Settings.Default.world = world.Text;
            //ConfigManager.write("ArmA3", "world", world.Text);
        }

        private void customParams_TextChanged(object sender, TextChangedEventArgs e) {
            Properties.Settings.Default.customParams = customParams.Text;
            //ConfigManager.write("ArmA3", "customParameters", customParams.Text);
        }

        private void Window_Closed(object sender, EventArgs e) {
            Properties.Settings.Default.Save();
            WindowManager.mainWindow.IsEnabled = true;
        }

        private void btn_addmod_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.gameversion == "ArmA3")
            {
                var window = new AddModsLaunchOptions(Properties.Settings.Default.ModPathA3.ToString());
                window.Show();
            }
            else if (Properties.Settings.Default.gameversion == "ArmA2"){
                var window = new AddModsLaunchOptions(Properties.Settings.Default.ModPathA2.ToString());
                window.Show();
            }
            btn_addmod.IsEnabled = false;
        }

        private void btn_cpyparams_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(getParams());
        }
        string getParams(){
            var world = Properties.Settings.Default.world;
            var customParams = Properties.Settings.Default.customParams;
            var mods = Modlister();

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
            if (world != "")
            {
                args += " -world=" + world;
            }
            if (customParams != "")
            {
                args += " " + cparams();
            }
            if (mods != "")
            {
                args += " -mod=\"" + mods + cParamsMods() + "\"";
            }
            return args;
        }
        string Modlister()
        {
            string modlist = "";
            string Path = path();

            foreach (Mod item in WindowManager.mainWindow.a3ModList.Items)
            {
                if (modlist == "")
                {
                    modlist = Path + item.modName + ";";
                }
                else
                {
                    modlist += Path + item.modName + ";";
                }
            }
            return modlist;
        }
        string path()
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
        private string cParamsMods()
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
        private string cparams()
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
    }
}
