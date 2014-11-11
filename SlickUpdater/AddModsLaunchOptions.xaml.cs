using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for AddModsLaunchOptions.xaml
    /// </summary>
    public partial class AddModsLaunchOptions : Window
    {
        public AddModsLaunchOptions(String SrcDir)
        {
            InitializeComponent();
            listfolders(SrcDir + "\\");
        }
        void listfolders(String SrcDir)
        {
            String[] dirs = Directory.GetDirectories(SrcDir);
            foreach (string dir in dirs)
            {
                if (dir.Replace(SrcDir, "")[0] == '@')
                {
                    folders.Items.Add(dir.Replace(SrcDir, ""));
                }
            }
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            List<String> list = new List<String>();
            for (int i = 0; i < folders.SelectedItems.Count; i++)
            {
                list.Add(folders.SelectedItems[i].ToString());
            }
            foreach (String dir in list)
            {
                if (!added.Items.Contains(dir))
                {
                    added.Items.Add(dir);
                }
            }
        }

        private void btn_removeitem_Click(object sender, RoutedEventArgs e)
        {
            added.Items.Remove(added.SelectedItem);
        }

        private void btn_done_Click(object sender, RoutedEventArgs e)
        {
            List<String> list = new List<String>();
            foreach (string mod in added.Items)
            {
                list.Add(mod);
            }
            String addedmods = String.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                addedmods += ("-mod=" + list[i]);
                if ((i - 1) < list.Count)
                {
                    addedmods += ";";
                }
                if (i == (list.Count - 1))
                {
                    addedmods += " ";
                }
            }
            Arma3LaunchOptionsDialogue.Instance.customParams.Text = addedmods + Arma3LaunchOptionsDialogue.Instance.customParams.Text.ToString();
            Arma3LaunchOptionsDialogue.Instance.btn_addmod.IsEnabled = true;
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Arma3LaunchOptionsDialogue.Instance.btn_addmod.IsEnabled = true;
        }

    }
}
