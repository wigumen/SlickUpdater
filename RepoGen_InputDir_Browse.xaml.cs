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
    /// Interaction logic for RepoGen_InputDir_Browse.xaml
    /// </summary>
    public partial class RepoGen_InputDir_Browse : Window {
        public RepoGen_InputDir_Browse() {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void setDirectoryButton_Click(object sender, RoutedEventArgs e) {
            WindowManager.repoGen_Options.inputDir_textBox.Text = Browser.Source.ToString();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Browser.Source = new Uri(WindowManager.repoGen_Options.inputDir_textBox.Text);
        }

        private void backButton_Click(object sender, RoutedEventArgs e) {
            try {
                Browser.GoBack();
            } catch (Exception) { }
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e) {
            try {
                Browser.GoForward();
            } catch (Exception) {

            }
        }

        private void upButton_Click(object sender, RoutedEventArgs e) {
            string source = Browser.Source.ToString();
            int index = source.LastIndexOf('/');
            Browser.Source = new Uri(source.Substring(0, index));
        }

        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            if (this.IsLoaded) {
                if (dirTextBox.Text != null) {
                    dirTextBox.Text = Browser.Source.ToString();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            WindowManager.repoGen_Options.IsEnabled = true;
        }
    }
}
