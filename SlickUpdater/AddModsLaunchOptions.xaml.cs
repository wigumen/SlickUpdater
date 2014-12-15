using System;
using System.Collections.Generic;
using System.IO;
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
            getLOMods();
        }
        /// <summary>
        /// Lists all folders from a path and adds modfolders to listview called "folders".
        /// </summary>
        /// <param name="SrcDir">Path to the directory the method will scan.</param>
        void listfolders(String SrcDir)
        {
            try
            {
                //Gets all directories in SrcDir
                String[] dirs = Directory.GetDirectories(SrcDir);

                //Adds all folders which begins with @ to the folderlist
                foreach (string dir in dirs)
                {
                    if (dir.Replace(SrcDir, "")[0] == '@')
                    {
                        folders.Items.Add(dir.Replace(SrcDir, ""));
                    }
                }
            }
            catch (Exception ex)
            {
                logIt.add("Error in Window [AddModsLaunchOptions]: " + ex.ToString());
                MessageBox.Show("Error Occurred: \n" + ex.ToString());
            }
        }
        /// <summary>
        /// Scans Arma3LaunchOptionsDialogoue custom parameters and gets all the mods and adds them to the listview called "added".
        /// </summary>
        void getLOMods()
        {
            //Saves the whole custom params to a string.
            String tmp = Arma3LaunchOptionsDialogue.Instance.customParams.Text;
            //Creates a regex to scan tmp with.
            //Regex rgex = new Regex("@[A-z0-9_]*");
            Regex rgex = new Regex("(@[A-z0-9_\\:]*|C:\\@[A-z0-9_\\:]*)");
            MatchCollection coll = rgex.Matches(tmp);

            //Adds all of the matched results.
            for (int i = 0; i < coll.Count; i++)
            {
                added.Items.Add(coll[i].ToString().Replace(";", ""));
                logIt.add(coll[i].ToString());
            }
        }
        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            List<String> list = new List<String>();

            //Adds every selected item to a list
            for (int i = 0; i < folders.SelectedItems.Count; i++)
            {
                list.Add(folders.SelectedItems[i].ToString());
            }

            //Adds ever item in the list to the listview called "added"
            foreach (String dir in list)
            {
                //Checks if the item already exists
                if (!added.Items.Contains(dir) && !added.Items.Contains(dir.ToLower()))
                {
                    added.Items.Add(dir);
                }
            }
        }
        private void btn_removeitem_Click(object sender, RoutedEventArgs e)
        {
            //Just removes the selected item
            added.Items.Remove(added.SelectedItem);
        }
        private void btn_done_Click(object sender, RoutedEventArgs e)
        {
            String addedmods = String.Empty;
            List<String> list = new List<String>();

            //Adds every item from the listview called "added" to a list
            foreach (string mod in added.Items)
            {
                list.Add(mod);
            }

            //Adds all the mods to one string
            for (int i = 0; i < list.Count; i++)
            {
                addedmods += list[i] + ";";
            }
            //Checks if anything have changed
            if (added.Items.Count != 0)
            {
                addedmods = "-mod=" + addedmods;
            }
            //Gets the customParams textbox value and removes ";" and "-mod=" and saves to String tmp
            String tmp = "";
            tmp = Arma3LaunchOptionsDialogue.Instance.customParams.Text.Replace(";", "").Replace("-mod=", "");

            //Creates a regex to find all the mods in string tmp
            Regex rgx = new Regex("(@[A-z0-9_\\:]*|C:\\@[A-z0-9_\\:]*)");
            MatchCollection collection = rgx.Matches(tmp);

            //Removes all the modnames in the string tmp
            for (int i = 0; i < collection.Count; i++)
            {
                tmp = tmp.Replace(collection[i].ToString(), "");
            }

            //If the first character in the string tmp is NOT a whitespace it will add one
            if (tmp != "")
            {
                if (tmp[0] != ' ' && added.Items.Count != 0)
                {
                    addedmods += " ";
                }
            }

            //Replaces the old text with the new one
            Arma3LaunchOptionsDialogue.Instance.customParams.Text = (addedmods + tmp);

            //Enables the "add specific mod"-button on Arma3LaunchOptionsDialogue
            Arma3LaunchOptionsDialogue.Instance.btn_addmod.IsEnabled = true;
            
            //Closes the window (duh)
            this.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //If the user closes window the "add specific mod"-button will be enabled again
            Arma3LaunchOptionsDialogue.Instance.btn_addmod.IsEnabled = true;
        }
    }
}
