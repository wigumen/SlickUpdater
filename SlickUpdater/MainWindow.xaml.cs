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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

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
        public string slickVersion = "1.4.0.1";
        private DispatcherTimer timer;
        List<MenuItem> items = new List<MenuItem>();
        public versionfile slickversion;
        string subreddit = "/r/ProjectMilSim";
        public double downloadedBytes = 1;
        string title = "Slick Updater";
        string downloadProgress = "";
        string time = "";
        public string currentGame = "Arma 3";

        public MainWindow()
        {
            string rawSlickJson = downloader.webRead("http://arma.projectawesome.net/beta/repo/slickupdater/slickversion.json");
            slickversion = JsonConvert.DeserializeObject<versionfile>(rawSlickJson);
            InitializeComponent();
            //First launch message!
            if(Properties.Settings.Default.firstLaunch == true)
            {
                MessageBox.Show("Hello! This seems to be the first time you launch SlickUpdater so make sure your arma 3 and ts3 path is set correctly in options. Have a nice day!", "Welcome");
                //Properties.Settings.Default.firstLaunch = false;
            }
            logThread = new logIt();
            repoHide();
            FileStream fs = new FileStream("localversion", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(slickVersion);
            sw.Close();
#if DEBUG
            //local debug server for A2 
            rawslickServVer = downloader.webRead("http://localhost/repo/slickupdater/slickversion");
#endif
            //Timer callback stuff for clock

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
            initRepos();
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

            //Init timer
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(updateTime);
            timer.Interval = new TimeSpan(0, 0, 20);
            timer.Start();

            WindowManager.SetWnd(this);

            a3DirText.Text = regcheck.arma3RegCheck();
            a2DirText.Text = regcheck.arma2RegCheck();
            va2DirText.Text = regcheck.varma2RegCheck();
            ts3DirText.Text = regcheck.ts3RegCheck();
            Properties.Settings.Default.firstLaunch = false;
            initProperties();
            logocheck();
        }

        private void initProperties()
        {
            var gameversion = Properties.Settings.Default.gameversion;
            a2DirText.Text = Properties.Settings.Default.A2path;
            a3DirText.Text = Properties.Settings.Default.A3path;
            ts3DirText.Text = Properties.Settings.Default.ts3Dir;
            if (gameversion == "ArmA3")
            {
                subreddit = slickversion.repos[repomenu.SelectedIndex].subreddit;
                joinButton.Content = slickversion.repos[repomenu.SelectedIndex].joinText;
                //changeGameButton.Content = "Arma 3";
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
            else
            {
                MessageBox.Show("Game version dun goofed! Please report issue to wigumen");
            }
        }

        private void updateTitle()
        {
            Title = time + " GMT" + " | " + title + downloadProgress ;
        }

        private void updateTime(object obj, EventArgs e)
        {
            time = DateTime.UtcNow.ToString("HH:mm");
            updateTitle();
        }

        private void setBusy(bool isBusy) {
            if (isBusy) {
                a3RefreshButton.IsEnabled = false;
                arma3Button.IsEnabled = false;
                joinButton.IsEnabled = false;
                repomenu.IsEnabled = false;
            } else if (!isBusy) {
                a3RefreshButton.IsEnabled = true;
                arma3Button.IsEnabled = true;
                joinButton.IsEnabled = true;
                repomenu.IsEnabled = true;
            }
        }
        private void onArma3Clicked(object sender, RoutedEventArgs e) {
            var gameversion = Properties.Settings.Default.gameversion;
            if (arma3Button.Content as string == "Update Arma 3" || arma3Button.Content as string == "Update Arma 2")
            {
                if (!worker.IsBusy) {
                    dlSpeedTimer = new System.Timers.Timer(10000);
                    dlSpeedTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateDlSpeed);
                    dlSpeedTimer.Start();
                    setBusy(true);
                    worker.RunWorkerAsync();
                } else {
                    MessageBox.Show("Worker is Busy(You really must be dicking around or unlucky to make this pop up...)");
                }
            }
            else if (currentGame == "Arma 3")
            {
                Launch.a3Launch(false, null, null);
            }
            else if (currentGame == "Arma 2")
            {
                Launch.a2Launch(false, null, null);
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
            Properties.Settings.Default.A3path = a3DirText.Text;
            Properties.Settings.Default.Save();
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
            time = DateTime.UtcNow.ToString("HH:mm");
            updateTitle();
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

        System.Timers.Timer dlSpeedTimer;

        private void worker_DoWork(object sender, DoWorkEventArgs e) {
            a3UpdateManager.a3Update();
        }
        double lastDownloadedBytes = 0;
        DateTime lastUpdateTime;
        private void updateDlSpeed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan intverval = now - lastUpdateTime;
            double timeDiff = intverval.TotalSeconds;
            double sizeDiff = downloadedBytes - lastDownloadedBytes;
            double downloadSpeed = (int)Math.Floor((sizeDiff) / timeDiff);
            downloadSpeed = downloadSpeed / 1048576;
            lastDownloadedBytes = downloadedBytes;
            lastUpdateTime = now;
            downloadProgress = " @ " + downloadSpeed.ToString("0.000") + " MB/s";
            this.Dispatcher.Invoke((Action)(() =>
            {
                WindowManager.mainWindow.updateTitle();
            }));
            
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
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            a3UpdateCheck();
            indivProgress.Value = 0;
            midProgress.Value = 0;
            totalProgress.Value = 0;
            midProgressTxt.Content = "";
            indivProgressTxt.Content = "";
            totalProgressTxt.Content = "";
            downloadProgress = "";
            dlSpeedTimer.Stop();
            updateTitle();
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
            if (nyanClick >= 15)
            {
                suIcon.Visibility = Visibility.Hidden;
                nyan.Visibility = Visibility.Visible;
            }
        }
        #endregion

        private void LaunchAndJoin(object sender, RoutedEventArgs e)
        {
            if (currentGame == "Arma 2")
            {
                var gameversion = Properties.Settings.Default.gameversion;
                var server = slickversion.repos[repomenu.SelectedIndex].server;
                var password = slickversion.repos[repomenu.SelectedIndex].password;
                Launch.a2Launch(true, server, password);
            } else if (currentGame == "Arma 3")
            {
                var gameversion = Properties.Settings.Default.gameversion;
                var server = slickversion.repos[repomenu.SelectedIndex].server;
                var password = slickversion.repos[repomenu.SelectedIndex].password;
                Launch.a3Launch(true, server, password);
            }
        }

        private void initRepos()
        {
            //List<ComboBoxItem> repos = new List<ComboBoxItem>();
            if (Properties.Settings.Default.A3repo != "")
            {
                repomenu.SelectedIndex = int.Parse(Properties.Settings.Default.A3repo);
            }

            foreach (Repos repo in slickversion.repos)
            {
                string Game = "";
                if (repo.game == "arma2")
                {
                    Game = "Arma 2";
                } else if (repo.game == "arma3")
                {
                    Game = "Arma 3";
                }
                    ComboBoxItem newItem = new ComboBoxItem();
                    newItem.Tag = repo.url;
                    newItem.Content = Game + " | " + repo.name;
                    newItem.MouseDown += setActiveRepo;
                    repomenu.Items.Add(newItem);
            }
        }

        private void setActiveRepo(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("IT WORKS OMG" + "     " + repomenu.SelectedIndex);
            if (slickversion.repos[repomenu.SelectedIndex].url == "not")
            {
                MessageBox.Show("This repo has not yet been implemented. Setting you to default");
                repomenu.SelectedIndex = 0;
                Properties.Settings.Default.A3repo = "" + 0;
                Properties.Settings.Default.A3repourl = slickversion.repos[0].url;
            }
            else
            {
                Properties.Settings.Default.A3repo = "" + repomenu.SelectedIndex;
                Properties.Settings.Default.A3repourl = slickversion.repos[repomenu.SelectedIndex].url;
            }
            logocheck();
            if (repomenu.IsDropDownOpen == true)
                {
                    a3UpdateCheck();
                }
        }

        private void refreshEvents(object sender, RoutedEventArgs e)
        {
            eventbox.Items.Clear();
            rposts.Clear();
            redditWorker.RunWorkerAsync();
            eventbutton.IsEnabled = false;
        }
        //logo change
        void logocheck()
        {
            if (slickversion.repos[repomenu.SelectedIndex].game == "arma2")
            {
                logo_image.Source = new BitmapImage(new Uri(@"Resources/ArmA2.png", UriKind.Relative));
                mainTab.Header = "Arma 2";
                currentGame = "Arma 2";
                a3UpdateManager.isArma2 = true;
            }
            else
            {
                logo_image.Source = new BitmapImage(new Uri(@"Resources/ArmA3.png", UriKind.Relative));
                mainTab.Header = "Arma 3";
                currentGame = "Arma 3";
                a3UpdateManager.isArma2 = false;
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

        private void ChangeGame(object sender, RoutedEventArgs e)
        {
            var gameversion = Properties.Settings.Default.gameversion;
            if (gameversion == "ArmA3")
            {
                Properties.Settings.Default.gameversion = "ArmA2";
            }
            if (gameversion == "ArmA2")
            {
                Properties.Settings.Default.gameversion = "ArmA3";
            }
        }

        private void a2DirText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.A2path = a2DirText.Text;
            Properties.Settings.Default.Save();
        }

        private void va2DirText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.vA2Path = va2DirText.Text;
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
