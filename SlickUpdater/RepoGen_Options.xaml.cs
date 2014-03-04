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
    /// Interaction logic for RepoGen_Options.xaml
    /// </summary>
    public partial class RepoGen_Options : Window {
        public RepoGen_Options() {
            WindowManager.repoGen_Options_setWnd(this);
            InitializeComponent();
        }

        private void inputDir_Button_Click(object sender, RoutedEventArgs e) {
            RepoGen_InputDir_Browse Browse = new RepoGen_InputDir_Browse();
            Browse.Show();
            repoGenWindow.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            string input = ConfigManager.fetch("repoGen", "inputDir");
            string output = ConfigManager.fetch("repoGen", "outputDir");
            
            //If configfile is blank make input arma 3 path, else make input from config file.
            if (input == "") {
                input = regcheck.arma3RegCheck();
            }
            inputDir_textBox.Text = input;

            //If config file is blank make it arma3path+\Slick Updater RepoGen, else make input configfile
            if (output == "") {
                output = regcheck.arma3RegCheck() + "\\Slick Updater RepoGen";
            }
            outputDir_textBox.Text = output;

        }

        private void outputDir_Button_Click(object sender, RoutedEventArgs e) {
            RepoGen_OutputDir_Browse Browse = new RepoGen_OutputDir_Browse();
            Browse.Show();
            repoGenWindow.IsEnabled = false;
        }

        private void inputDir_textBox_TextChanged(object sender, TextChangedEventArgs e) {
            ConfigManager.write("repoGen", "inputDir", inputDir_textBox.Text);
        }

        private void outputDir_textBox_TextChanged(object sender, TextChangedEventArgs e) {
            ConfigManager.write("repoGen", "outputDir", outputDir_textBox.Text);
        }

        private void Window_Closed(object sender, EventArgs e) {
            WindowManager.mainWindow.IsEnabled = true;
        }
    }
}