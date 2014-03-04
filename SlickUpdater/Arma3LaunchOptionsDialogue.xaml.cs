using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Arma3LaunchOptionsDialogue() {
            InitializeComponent();
            initializeValues();
        }
        void initializeValues() {

            if (ConfigManager.fetch("ArmA3", "window") == "true") {
                window.IsChecked = true;
            } else {
                window.IsChecked = false;
            }

            if (ConfigManager.fetch("ArmA3", "nosplash") == "true") {
                nosplash.IsChecked = true;
            } else {
                nosplash.IsChecked = false;
            }

            if (ConfigManager.fetch("ArmA3", "skipIntro") == "true") {
                skipIntro.IsChecked = true;
            } else {
                skipIntro.IsChecked = false;
            }

            if (ConfigManager.fetch("ArmA3", "noLogs") == "true") {
                noLogs.IsChecked = true;
            } else {
                noLogs.IsChecked = false;
            }

            if (ConfigManager.fetch("ArmA3", "noPause") == "true") {
                noPause.IsChecked = true;
            } else {
                noPause.IsChecked = false;
            }

            if (ConfigManager.fetch("ArmA3", "showScriptErrors") == "true") {
                showScriptErrors.IsChecked = true;
            } else {
                showScriptErrors.IsChecked = false;
            }

            world.Text = ConfigManager.fetch("ArmA3", "world");
            customParams.Text = ConfigManager.fetch("ArmA3", "customParameters");
        }

        private void window_Click(object sender, RoutedEventArgs e) {
            if (window.IsChecked == true) {
                ConfigManager.write("ArmA3", "window", "true");
            } else {
                ConfigManager.write("ArmA3", "window", "false");
            }
        }

        private void nosplash_Click(object sender, RoutedEventArgs e) {
            if (nosplash.IsChecked == true) {
                ConfigManager.write("ArmA3", "nosplash", "true");
            } else {
                ConfigManager.write("ArmA3", "nosplash", "false");
            }
        }

        private void skipIntro_Click(object sender, RoutedEventArgs e) {
            if (skipIntro.IsChecked == true) {
                ConfigManager.write("ArmA3", "skipIntro", "true");
            } else {
                ConfigManager.write("ArmA3", "skipIntro", "false");
            }
        }

        private void noLogs_Click(object sender, RoutedEventArgs e) {
            if (noLogs.IsChecked == true) {
                ConfigManager.write("ArmA3", "noLogs", "true");
            } else {
                ConfigManager.write("ArmA3", "noLogs", "false");
            }
        }

        private void noPause_Click(object sender, RoutedEventArgs e) {
            if (noPause.IsChecked == true) {
                ConfigManager.write("ArmA3", "noPause", "true");
            } else {
                ConfigManager.write("ArmA3", "noPause", "false");
            }
        }

        private void showScriptErrors_Click(object sender, RoutedEventArgs e) {
            if (showScriptErrors.IsChecked == true) {
                ConfigManager.write("ArmA3", "showScriptErrors", "true");
            } else {
                ConfigManager.write("ArmA3", "showScriptErrors", "false");
            }
        }

        private void world_TextChanged(object sender, TextChangedEventArgs e) {
            ConfigManager.write("ArmA3", "world", world.Text);
        }

        private void customParams_TextChanged(object sender, TextChangedEventArgs e) {
            ConfigManager.write("ArmA3", "customParameters", customParams.Text);
        }

        private void Window_Closed(object sender, EventArgs e) {
            WindowManager.mainWindow.IsEnabled = true;
        }
    }
}
