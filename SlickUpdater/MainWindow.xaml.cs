using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace SlickUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BackgroundWorker worker;
        public BackgroundWorker checkWorker;
        public BackgroundWorker redditWorker;
        public logIt logThread;
        public string slickVersion = "1.3";
        List<MenuItem> items = new List<MenuItem>();
        string rawslickServVer;
        string[] slickServVer;
        versionfile slickversion;
        string subreddit = "/r/ProjectMilSim";
        public double downloadedBytes = 1;
        Stopwatch sw = new Stopwatch();

        public MainWindow()
        {
            string rawSlickJson = downloader.webRead("http://arma.projectawesome.net/beta/repo/slickupdater/slickversion.json");
            slickversion = JsonConvert.DeserializeObject<versionfile>(rawSlickJson);
            InitializeComponent();
            //First launch message!
            if(Properties.Settings.Default.firstLaunch == true)
            {
                MessageBox.Show("Hello! This seems to be the first time you launch SlickUpdater so make sure your arma 3 and ts3 path is set correctly in options. Have a nice day!", "Welcome");
                Properties.Settings.Default.A3repourl = slickServVer[2];
                Properties.Settings.Default.firstLaunch = false;
            }
            logThread = new logIt();
            repoHide();
            FileStream fs = new FileStream("localversion", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(slickVersion);
            sw.Close();
            menuAnimation(140, 0);
            showRepo = false;
            rawslickServVer = downloader.webRead("http://projectawesomemodhost.com/beta/repo/slickupdater/slickversion");
#if DEBUG
            //local debug server for A2 
            rawslickServVer = downloader.webRead("http://localhost/repo/slickupdater/slickversion");
#endif
            slickServVer = rawslickServVer.Split('%');
            MenuItem pa = new MenuItem();
            pa.Tag = "http://projectawesomemodhost.com/beta/repo/";
            pa.Header = "PA Repo";
            items.Add(pa);
            if (slickversion.version != slickVersion)
            {
                MessageBoxResult result = MessageBox.Show("There seems to be a new version of slickupdater available, do you wanna update it it?", "New Update", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        System.Diagnostics.Process.Start("SlickAutoUpdate.exe");
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                        break;
                    case MessageBoxResult.No:

                        break;
                }
            }

            // Initialize Update Worker
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            //init checkWorker
            checkWorker = new BackgroundWorker();
            checkWorker.DoWork += checkWorker_DoWork;
            checkWorker.ProgressChanged += checkWorker_ProgressChanged;
            checkWorker.WorkerReportsProgress = true;
            checkWorker.RunWorkerCompleted += checkWorker_RunWorkerCompleted;

            //reddit worker
            redditWorker = new BackgroundWorker();
            redditWorker.DoWork += redditWorker_DoWork;
            redditWorker.RunWorkerCompleted += redditworker_Done;

            WindowManager.SetWnd(this);
            //Check if the user if a PA user or a TEST user
            var gameversion = Properties.Settings.Default.gameversion;
            if (gameversion == "ArmA3")
            {
                a3DirText.Text = regcheck.arma3RegCheck();
                ts3DirText.Text = regcheck.ts3RegCheck();
                menuButton.Content = Properties.Settings.Default.A3repo;
                var subredd = Properties.Settings.Default.A3repo;
                if (subredd == "PA Repo")
                {
                    subreddit = "/r/ProjectMilSim";
                    joinButton.Content = "Join PA server";
                }else if (subredd == "Test Outfit Repo")
                {
                    subreddit = "/r/testoutfit";
                    joinButton.Content = "Join TEST server";
                }
            }
            else
            {
                var subredd = Properties.Settings.Default.A2repo;
                if (subredd == "PA ArmA 2 Repo")
                {
                    subreddit = "/r/ProjectMilSim";
                    joinButton.Content = "Join PA ArmA 2 server";
                }
            }

        }
        //Do some work
        void checkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (checkWorker.IsBusy) return;
            if (worker.IsBusy) return;
            setBusy(false);
        }
        //check if da shit is up to date
        void a3UpdateCheck() {
            if (!checkWorker.IsBusy) {
                setBusy(true);
                checkWorker.RunWorkerAsync();
            } else {
                MessageBox.Show("checkWorker is Busy!");
            }
        }
        
        void checkWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            switch (e.ProgressPercentage) {
                case -1:
                    MessageBox.Show(e.UserState as String);
                    break;
                case 1:
                    arma3Button.Content = e.UserState as String;
                    break;
                case 2:
                    a3ModList.ItemsSource = e.UserState as List<Mod>;
                    break;

            }
        }
        // worker runs the updateManager, checks game version using <GameVER><Game>
        void checkWorker_DoWork(object sender, DoWorkEventArgs e) {
            var gameversion = Properties.Settings.Default.gameversion;
            if (gameversion == "ArmA3")
            {
                
                a3UpdateManager.arma3UpdateCheck();
            }
            else if (gameversion == "ArmA2")
            {
                a2UpdateManager.arma2UpdateCheck();
            }
            else
            {
                MessageBox.Show("Game version dun goofed! Please report issue to wigumen");
            }
        }

        private void setBusy(bool isBusy) {
            if (isBusy) {
                a3RefreshButton.IsEnabled = false;
                arma3Button.IsEnabled = false;
                joinButton.IsEnabled = false;
                menuButton.IsEnabled = false;
            } else if (!isBusy) {
                a3RefreshButton.IsEnabled = true;
                arma3Button.IsEnabled = true;
                joinButton.IsEnabled = true;
                menuButton.IsEnabled = true;
            }
        }
        private void onArma3Clicked(object sender, RoutedEventArgs e) {
            var gameversion = Properties.Settings.Default.gameversion;
            if (arma3Button.Content as string == "Update ArmA 3" || arma3Button.Content as string == "Update ArmA 2")
            {
                if (!worker.IsBusy) {
                    setBusy(true);
                    worker.RunWorkerAsync();
                } else {
                    MessageBox.Show("Worker is Busy(You really must be dicking around or unlucky to make this pop up...)");
                }
            }
            else if (gameversion == "ArmA3")
            {
                Launch.a3Launch(false, "");
            }
            else
            {
                Launch.a2Launch(false, "");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void a3RefreshButton_Click(object sender, RoutedEventArgs e) {
            if (!checkWorker.IsBusy) {
                setBusy(true);
                a3UpdateCheck();
            } else {
                MessageBox.Show("checkWorker thread is currently busy...");
            }

        }

        private void a3RefreshImageEnter(object sender, MouseEventArgs e) {
            DoubleAnimation rotationAnimation = new DoubleAnimation();

            rotationAnimation.From = 0;
            rotationAnimation.To = 360;
            rotationAnimation.Duration = new Duration(TimeSpan.FromSeconds(.5));
            rotationAnimation.AccelerationRatio = 0.3;
            rotationAnimation.DecelerationRatio = 0.3;

            Storyboard storyboard = new Storyboard();

            Storyboard.SetTarget(rotationAnimation, refreshImage);
            Storyboard.SetTargetProperty(rotationAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)"));
            storyboard.Children.Add(rotationAnimation);


            this.BeginStoryboard(storyboard);
        }

        private void launchOptionsButton_Click(object sender, RoutedEventArgs e) {
            Arma3LaunchOptionsDialogue dialogue = new Arma3LaunchOptionsDialogue();
            dialogue.Show();
            mainWindow.IsEnabled = false;
        }

        private void a3DirText_TextChanged(object sender, TextChangedEventArgs e) {
            Properties.Settings.Default.path = a3DirText.Text;
        }

        private void a3Ts3Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.ts3Dir = ts3DirText.Text;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            setBusy(true);
            a3UpdateCheck();
            redditWorker.RunWorkerAsync();
            eventbutton.IsEnabled = false;
        }

        private void repoGen_Options_Click(object sender, RoutedEventArgs e) {
            RepoGen_Options repoGen = new RepoGen_Options();
            repoGen.Show();
            mainWindow.IsEnabled = false;
        }

        private void inputDirListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragAndDrop.startPoint = e.GetPosition(null);
        }

        private void inputDirListBox_PreviewMouseMove(object sender, MouseEventArgs e) {
            DragAndDrop.inputDirListBox_PreviewMouseMove(sender, e);
        }

        private void outputDirListBox_DragEnter(object sender, DragEventArgs e) {
            DragAndDrop.outputDirListBox_DragEnter(sender, e);
        }

        private void outputDirListBox_Drop(object sender, DragEventArgs e) {
            DragAndDrop.outputDirListBox_Drop(sender, e);
        }

        private void repoGen_Refresh_Click(object sender, RoutedEventArgs e) {
            RepoGenerator.inputGen();
        }

        private void inputDirListBox_DragEnter(object sender, DragEventArgs e) {
            DragAndDrop.inputDirListBox_DragEnter(sender, e);
        }

        private void inputDirListBox_Drop(object sender, DragEventArgs e) {
            DragAndDrop.inputDirListBox_Drop(sender, e);
        }

        private void outputDirListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragAndDrop.startPoint = e.GetPosition(null);
        }

        private void outputDirListBox_PreviewMouseMove(object sender, MouseEventArgs e) {
            DragAndDrop.outputDirListBox_PreviewMouseMove(sender, e);
        }

        private void repoGenButton_Click(object sender, RoutedEventArgs e) {
            mainWindow.IsEnabled = false;
            RepoGenerator.startGen();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e) {
            sw.Start();
            var gameversion = Properties.Settings.Default.gameversion;
            if (gameversion == "ArmA3")
            {
                a3UpdateManager.a3Update();
            }
            else
            {
                a2UpdateManager.a2Update();
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.ProgressPercentage <= 100 && e.ProgressPercentage >= 0) {
                indivProgress.Value = e.ProgressPercentage;
                indivProgressTxt.Content = e.ProgressPercentage + "%";

            } else if (e.ProgressPercentage > 100 && e.ProgressPercentage <= 201) {
                midProgressTxt.Content = e.ProgressPercentage - 101 + "%";
                midProgress.Value = e.ProgressPercentage - 101;
            } else if (e.ProgressPercentage > 201 && e.ProgressPercentage <= 302) {
                totalProgressTxt.Content = e.ProgressPercentage - 202 + "%";
                totalProgress.Value = e.ProgressPercentage - 202;
            } else if (e.ProgressPercentage == -1) {
                MessageBox.Show(e.UserState as string);
            }
            double downloadSpeed = downloadedBytes / 1048576 / sw.Elapsed.TotalMilliseconds * 1000;
            Title = "Slick Updater Beta @ " + downloadSpeed.ToString("0.00#") + " Mb/s";
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            sw.Stop();
            a3UpdateCheck();
            indivProgress.Value = 0;
            midProgress.Value = 0;
            totalProgress.Value = 0;
            midProgressTxt.Content = "";
            indivProgressTxt.Content = "";
            totalProgressTxt.Content = "";
            Title = "Slick Updater Beta";
            
        }

        private void helpButton_Click(object sender, RoutedEventArgs e) {
            mainWindow.IsEnabled = false;
            About abt = new About();
            abt.Show();
        }

        private void logging_click(object sender, RoutedEventArgs e)
        {
            log logging = new log();
            logging.Show();
        }

        private void forceToggle(object sender, RoutedEventArgs e)
        {
            var currepourl = Properties.Settings.Default.A3repourl;
            string[] modlist = downloader.webReadLines(currepourl + "modlist.cfg");
            MessageBoxResult result = MessageBox.Show("This will delete your mods and redownload them are you sure?", "You 100% sure?", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    string msg = "Removed mods";
                    forceButton.Content = msg;
                    forceButton.Width = 90;
                    string a3path = regcheck.arma3RegCheck();
                    foreach (string modline in modlist)
                    {
                        try
                        {
                            if(Directory.Exists(a3path + "\\" + modline)){
                                logIt.addData("Deleted " + modline);
                                Directory.Delete(a3path + "\\" + modline, true);
                            }
                        }
                        catch (IOException) { }
                    }
                    break;
                case MessageBoxResult.No:

                    break;
            }
        }

        private void repoHide()
        {
            repoGen.Visibility = Visibility.Hidden;
        }

        int clickCount = 0;

        private void showrepo(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (clickCount > 4)
            {
                repoGen.Visibility = Visibility.Visible;
            }
        }

        
        #region TOPSECRET EASTEREGG NO PEAKING
        int nyanClick = 0;
        private void nyanEgg(object sender, MouseButtonEventArgs e)
        {
            nyanClick++;
            if (nyanClick > 14)
            {
                suIcon.Visibility = Visibility.Hidden;
                nyan.Visibility = Visibility.Visible;
            }
        }
        #endregion

        private void LaunchAndJoin(object sender, RoutedEventArgs e)
        {
            var gameversion = Properties.Settings.Default.gameversion;
            if (gameversion == "ArmA3")
            {
                var server = Properties.Settings.Default.A3repo;
                Launch.a3Launch(true, server);
            }
            else
            {
                Launch.a2Launch(true, "PA Repo");
            }
        }
        bool showRepo = false;
        private void showRepos(object sender, RoutedEventArgs e)
        {
            if (showRepo == false)
            {
                menuAnimation(0, 140);
                //repomenu.Visibility = Visibility.Visible; 
                showRepo = true;
            }
            else {
                menuAnimation(140, 0);
                showRepo = false;
            }

        }

        private void setActiveRepo(object sender, RoutedEventArgs e)
        {
            MenuItem obj = sender as MenuItem;
            if (obj.Tag.ToString() == "pa")
            {
                menuButton.Content = obj.Header;
                menuAnimation(140, 0);
                Properties.Settings.Default.A3repourl = slickServVer[2];
                Properties.Settings.Default.A3repo = obj.Header.ToString();
                Properties.Settings.Default.gameversion = "ArmA3";
                joinButton.Content = "Join PA server";
                subreddit = "/r/ProjectMilSim";
            }
            if (obj.Tag.ToString() == "test")
            {
                menuButton.Content = obj.Header;
                menuAnimation(140, 0);
                Properties.Settings.Default.A3repourl = slickServVer[3];
                Properties.Settings.Default.A3repo = obj.Header.ToString();
                Properties.Settings.Default.gameversion = "ArmA3";
                joinButton.Content = "Join TEST server";
                subreddit = "/r/TestOutfit";
            }
            if (obj.Tag.ToString() == "paExtra")
            {
                if (slickServVer[4] == "not")
                {
                    MessageBox.Show("This repo has not yet been implemented, setting you to PA repo");
                    menuButton.Content = "PA Repo";
                    menuAnimation(140, 0);
                    Properties.Settings.Default.A3repourl = slickServVer[2];
                    Properties.Settings.Default.A3repo = obj.Header.ToString();
                    joinButton.Content = "Join PA server";
                    subreddit = "/r/ProjectMilSim";
                }
                else
                {
                    menuButton.Content = obj.Header;
                    menuAnimation(140, 0);
                    Properties.Settings.Default.A3repourl =  slickServVer[4];
                    Properties.Settings.Default.A3repo = obj.Header.ToString();
                    joinButton.Content = "Join PA server";
                    subreddit = "/r/ProjectMilSim";
                }
            }
            if (obj.Tag.ToString() == "paarama2")
            {
                menuButton.Content = obj.Header;
                menuAnimation(140, 0);
                Properties.Settings.Default.A2repo = slickServVer[5];
                Properties.Settings.Default.A2current = obj.Header.ToString();
                Properties.Settings.Default.gameversion = "ArmA2";
                joinButton.Content = "Join PA ArmA 2 server";
                subreddit = "/r/ProjectMilsim";
            }
                showRepo = false;
                if (!checkWorker.IsBusy)
                {
                    setBusy(true);
                    a3UpdateCheck();
                }
                else
                {
                    MessageBox.Show("checkWorker thread is currently busy...");
                }
        }

        private void repoLostFocus(object sender, MouseButtonEventArgs e)
        {
            menuAnimation(140, 0);
            showRepo = false;
        }

        void menuAnimation(int from, int to)
        {
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = from;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromSeconds(.3));
            animation.AccelerationRatio = 0.3;
            animation.DecelerationRatio = 0.3;

            Storyboard storyboard = new Storyboard();

            Storyboard.SetTarget(animation, repomenu);

            Storyboard.SetTargetProperty(animation,
                new PropertyPath("Height", repomenu));
            storyboard.Children.Add(animation);


            this.BeginStoryboard(storyboard);
            //repomenu.Visibility = Visibility.Hidden; 
            showRepo = false;
        }
        
        void addNewMenuItem(string repoPath, string name)
        {
            MenuItem newItem = new MenuItem();
            newItem.Header = name;
            newItem.Tag = repoPath;
            //newItem.Click += new EventHandler(this.setActiveRepo);
            items.Add(newItem);
        }

        private void refreshEvents(object sender, RoutedEventArgs e)
        {
            eventbox.Items.Clear();
            rposts.Clear();
            redditWorker.RunWorkerAsync();
            eventbutton.IsEnabled = false;
        }
        //logo change
        void logocheck(String gameversion)
        {
            if (gameversion == "ArmA2")
            {
                
                logo_image.Source = new BitmapImage(new Uri(@"Resources/ArmA2.png", UriKind.Relative));
            }
            else
            {
                logo_image.Source = new BitmapImage(new Uri(@"Resources/ArmA3.png", UriKind.Relative));
            }
        }

        List<events> rposts = new List<events>();

        void redditWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            string url = @"http://www.reddit.com" + subreddit + "/hot.json";
            string json = downloader.webRead(url);
            RootObject topic = JsonConvert.DeserializeObject<RootObject>(json);
            
            foreach(Child i in topic.data.children)
            {
                if (i.data.link_flair_text == "EVENT")
                {
                    events evt = new events();
                    evt.title = i.data.title;
                    evt.author = i.data.author;
                    evt.url = i.data.permalink;
                    rposts.Add(evt);
                }
            }
             
        }

        void redditworker_Done(object sender, AsyncCompletedEventArgs e)
        {
            foreach (events evn in rposts)
            {
                
                Button newEvent = new Button();
                newEvent.Content = evn.title + " by " + evn.author;
                newEvent.Height = 50;
                newEvent.Width = 520;
                newEvent.Tag = evn.url;
                newEvent.FontSize = 14;
                newEvent.Click += newEvent_Click;
                eventbox.Items.Add(newEvent);
             }
            eventbutton.IsEnabled = true;
        }

        void newEvent_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            System.Diagnostics.Process.Start("http://www.reddit.com" + button.Tag.ToString());
        }
        void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
    public class Mod {
        public ImageSource status { get; set; }

        public string modName { get; set; }
        public string version { get; set; }
        public string servVersion { get; set; }
    }

    public class events
    {
        public string title { get; set; }
        public string author { get; set; }
        public string url { get; set; }
    }
}
