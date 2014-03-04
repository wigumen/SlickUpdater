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

namespace SlickUpdater
{
    /// <summary>
    /// Interaction logic for newRepo.xaml
    /// </summary>
    public partial class newRepo : Window
    {
        string path = "";
        string name = "";
        string state = "";

        public newRepo()
        {
            InitializeComponent();
        }

        private void openFileBrowser(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                path = dialog.FileName;
                pathbox.Text = path;
            }
        }

        private void updatePath(object sender, TextChangedEventArgs e)
        {
            path = pathbox.Text;
        }

        private void updateName(object sender, TextChangedEventArgs e)
        {
            name = NameBox.Text;
        }

        private void save(object sender, RoutedEventArgs e)
        {
            if (path != "" || name != "")
            {
                state = "true";
                this.Close();
            }
            else
            {
                MessageBox.Show("You need to fill out all the fields");
            }
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            state = "false";
            this.Close();
        }

        public string[] result()
        {
            string[] finalRes = { state, path, name };
            return finalRes;
        }
    }
}
