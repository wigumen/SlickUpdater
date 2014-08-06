using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using SlickUpdater.Properties;
using Button = System.Windows.Controls.Button;
using DragEventArgs = System.Windows.DragEventArgs;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Timer = System.Timers.Timer;

namespace SlickUpdater
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<events> _rposts = new List<events>();
        public BackgroundWorker CheckWorker;
        public string CurrentGame = "Arma 3";
        public double DownloadedBytes = 1;
        public logIt LogThread;
        public BackgroundWorker RedditWorker;
        public string SlickVersion = "1.4.0.1";
        public versionfile Slickversion;
        public BackgroundWorker Worker;
        private string _downloadProgress = "";
        private string _subreddit = "/r/ProjectMilSim";
        private string _time = "";
        private int clickCount;
        private Timer dlSpeedTimer;
        private double lastDownloadedBytes;
        private DateTime lastUpdateTime;
        private DispatcherTimer timer;
        private string title = "Slick Updater";

        public MainWindow()
        {
            string rawSlickJson = String.Empty;

            logIt.add("Starting app");

            //Check Command Line args
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-override")
                {
                    try
                    {
                        rawSlickJson = downloader.webRead(args[i + 1]);
                    }
                    catch (Exception e)
                    {
                        logIt.add("Could not override masterfile: " + e);
                    }
                }
            }
            if (rawSlickJson == String.Empty)
            {
                try
                {
#if DEBUG
    //local debug server for testing
                rawSlickJson = downloader.webRead("http://localhost/slickversion.json");
#else

                    //Default master file location hosted on Project Awesome servers
                    rawSlickJson = downloader.webRead("http://arma.projectawesome.net/beta/repo/slickupdater/slickversion.json");

                }
                catch (Exception ex)
                {
                    logIt.add("Error while downloading slickversion.json trying backup server:\n" + ex.ToString());
                }
                if (!String.IsNullOrEmpty(rawSlickJson))
                {
                    try
                    {
                        //Backup master file hosted on GitHub servers
                        rawSlickJson =
                            downloader.webRead(
                                "https://gist.githubusercontent.com/wigumen/015cb44774c6320cf901/raw/6a5f22437997c6c120a1b15beaabdb3ade3be06a/slickversion.json");
                    }
                    catch (Exception ex)
                    {
                        logIt.add("Error while trying to reach backup server going offline mode:\n" + ex.ToString());
                    }
                }
            }

            if (!String.IsNullOrEmpty(rawSlickJson))
            {
                Slickversion = JsonConvert.DeserializeObject<versionfile>(rawSlickJson);
            }
            else
            {
                // the Slickversion file couldn't be downloaded, create it ourselves.
                // Note: this means the data displayed in the app is not correct
                Slickversion = new versionfile();
            }
#endif
            InitializeComponent();
            //First launch message!
            if (Settings.Default.firstLaunch)
            {
                MessageBox.Show(
                    "Hello! This seems to be the first time you launch SlickUpdater so make sure your arma 3 and ts3 path is set correctly in options. Have a nice day!",
                    "Welcome");
                //Note to myself: I actualy set firstLaunch to false in initProps
            }
            LogThread = new logIt();
            repoHide();
            var fs = new FileStream("localversion", FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(fs);
            sw.WriteLine(SlickVersion);
            sw.Close();

            //Timer callback stuff for clock

            if (!String.IsNullOrEmpty(Slickversion.version) && !String.IsNullOrEmpty(SlickVersion) &&
                (Slickversion.version != SlickVersion))
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "There seems to be a new version of slickupdater available, do you wanna update it it?",
                        "New Update", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Process.Start("SlickAutoUpdate.exe");
                        Process.GetCurrentProcess().Kill();
                        break;
                    case MessageBoxResult.No:

                        break;
                }
            }

            initRepos();
            // Initialize Update Worker
            Worker = new BackgroundWorker();
            Worker.DoWork += worker_DoWork;
            Worker.ProgressChanged += worker_ProgressChanged;
            Worker.WorkerReportsProgress = true;
            Worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            //init checkWorker
            CheckWorker = new BackgroundWorker();
            CheckWorker.DoWork += checkWorker_DoWork;
            CheckWorker.ProgressChanged += checkWorker_ProgressChanged;
            CheckWorker.WorkerReportsProgress = true;
            CheckWorker.RunWorkerCompleted += checkWorker_RunWorkerCompleted;

            //reddit worker
            RedditWorker = new BackgroundWorker();
            RedditWorker.DoWork += redditWorker_DoWork;
            RedditWorker.RunWorkerCompleted += redditworker_Done;

            //Init timer
            timer = new DispatcherTimer();
            timer.Tick += updateTime;
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();

            WindowManager.SetWnd(this);

            a3DirText.Text = regcheck.arma3RegCheck();
            a2DirText.Text = regcheck.arma2RegCheck();
            va2DirText.Text = regcheck.varma2RegCheck();
            ts3DirText.Text = regcheck.ts3RegCheck();

            Settings.Default.firstLaunch = false;
            InitProperties();
            logocheck();

        }

        private void InitProperties()
        {
            a2DirText.Text = Settings.Default.A2path;
            a3DirText.Text = Settings.Default.A3path;
            ts3DirText.Text = Settings.Default.ts3Dir;
            if ((repomenu.SelectedIndex) < (Slickversion.repos.Count))
            {
                _subreddit = Slickversion.repos[repomenu.SelectedIndex].subreddit;
                joinButton.Content = Slickversion.repos[repomenu.SelectedIndex].joinText;
            }
            updateGuides(null, null);


        }

        //Do some work
        private void checkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CheckWorker.IsBusy) return;
            if (Worker.IsBusy) return;
            SetBusy(false);
        }

        //check if da shit is up to date
        private void a3UpdateCheck()
        {
            if (!CheckWorker.IsBusy)
            {
                SetBusy(true);
                CheckWorker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("checkWorker is Busy!");
            }
        }

        private void checkWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
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
        private void checkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateManager.UpdateCheck();
        }

        private void updateTitle()
        {
            Title = _time + " GMT" + " | " + title + _downloadProgress;
        }

        private void updateTime(object obj, EventArgs e)
        {
            _time = DateTime.UtcNow.ToString("HH:mm");
            updateTitle();
        }

        private void SetBusy(bool isBusy)
        {
            if (isBusy)
            {
                a3RefreshButton.IsEnabled = false;
                arma3Button.IsEnabled = false;
                joinButton.IsEnabled = false;
                repomenu.IsEnabled = false;
            }
            else if (!isBusy)
            {
                a3RefreshButton.IsEnabled = true;
                arma3Button.IsEnabled = true;
                joinButton.IsEnabled = true;
                repomenu.IsEnabled = true;
            }
        }

        private void OnArma3Clicked(object sender, RoutedEventArgs e)
        {
            string gameversion = Settings.Default.gameversion;
            if (arma3Button.Content as string == "Update Arma 3" || arma3Button.Content as string == "Update Arma 2")
            {
                if (!Worker.IsBusy)
                {
                    dlSpeedTimer = new Timer(10000);
                    dlSpeedTimer.Elapsed += updateDlSpeed;
                    dlSpeedTimer.Start();
                    SetBusy(true);
                    Worker.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show(
                        "Worker is Busy(You really must be dicking around or unlucky to make this pop up...)");
                }
            }
            else if (CurrentGame == "Arma 3")
            {
                Launch.a3Launch(false, null, null);
            }
            else if (CurrentGame == "Arma 2")
            {
                Launch.a2Launch(false, null, null);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void a3RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckWorker.IsBusy)
            {
                SetBusy(true);
                a3UpdateCheck();
            }
            else
            {
                MessageBox.Show("checkWorker thread is currently busy...");
            }
        }

        private void a3RefreshImageEnter(object sender, MouseEventArgs e)
        {
            var rotationAnimation = new DoubleAnimation();

            rotationAnimation.From = 0;
            rotationAnimation.To = 360;
            rotationAnimation.Duration = new Duration(TimeSpan.FromSeconds(.5));
            rotationAnimation.AccelerationRatio = 0.3;
            rotationAnimation.DecelerationRatio = 0.3;

            var storyboard = new Storyboard();

            Storyboard.SetTarget(rotationAnimation, refreshImage);
            Storyboard.SetTargetProperty(rotationAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)"));
            storyboard.Children.Add(rotationAnimation);


            BeginStoryboard(storyboard);
        }

        private void launchOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogue = new Arma3LaunchOptionsDialogue();
            dialogue.Show();
            mainWindow.IsEnabled = false;
        }

        private void a3DirText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.A3path = a3DirText.Text;
            Settings.Default.Save();
        }

        private void a3Ts3Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.ts3Dir = ts3DirText.Text;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetBusy(true);
            a3UpdateCheck();
            RedditWorker.RunWorkerAsync();
            eventbutton.IsEnabled = false;
            _time = DateTime.UtcNow.ToString("HH:mm");
            updateTitle();
        }

        private void repoGen_Options_Click(object sender, RoutedEventArgs e)
        {
            var repoGen = new RepoGen_Options();
            repoGen.Show();
            mainWindow.IsEnabled = false;
        }

        private void inputDirListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragAndDrop.startPoint = e.GetPosition(null);
        }

        private void inputDirListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            DragAndDrop.inputDirListBox_PreviewMouseMove(sender, e);
        }

        private void outputDirListBox_DragEnter(object sender, DragEventArgs e)
        {
            DragAndDrop.outputDirListBox_DragEnter(sender, e);
        }

        private void outputDirListBox_Drop(object sender, DragEventArgs e)
        {
            DragAndDrop.outputDirListBox_Drop(sender, e);
        }

        private void repoGen_Refresh_Click(object sender, RoutedEventArgs e)
        {
            RepoGenerator.inputGen();
        }

        private void inputDirListBox_DragEnter(object sender, DragEventArgs e)
        {
            DragAndDrop.inputDirListBox_DragEnter(sender, e);
        }

        private void inputDirListBox_Drop(object sender, DragEventArgs e)
        {
            DragAndDrop.inputDirListBox_Drop(sender, e);
        }

        private void outputDirListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragAndDrop.startPoint = e.GetPosition(null);
        }

        private void outputDirListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            DragAndDrop.outputDirListBox_PreviewMouseMove(sender, e);
        }

        private void repoGenButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.IsEnabled = false;
            RepoGenerator.startGen();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateManager.a3Update();
        }

        private void updateDlSpeed(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan intverval = now - lastUpdateTime;
            double timeDiff = intverval.TotalSeconds;
            double sizeDiff = DownloadedBytes - lastDownloadedBytes;
            double downloadSpeed = (int) Math.Floor((sizeDiff)/timeDiff);
            downloadSpeed = downloadSpeed/1048576;
            lastDownloadedBytes = DownloadedBytes;
            lastUpdateTime = now;
            _downloadProgress = " @ " + downloadSpeed.ToString("0.000") + " MB/s";
            Dispatcher.Invoke(() => WindowManager.mainWindow.updateTitle());
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage <= 100 && e.ProgressPercentage >= 0)
            {
                indivProgress.Value = e.ProgressPercentage;
                indivProgressTxt.Content = e.ProgressPercentage + "%";
            }
            else if (e.ProgressPercentage > 100 && e.ProgressPercentage <= 201)
            {
                midProgressTxt.Content = e.ProgressPercentage - 101 + "%";
                midProgress.Value = e.ProgressPercentage - 101;
            }
            else if (e.ProgressPercentage > 201 && e.ProgressPercentage <= 302)
            {
                totalProgressTxt.Content = e.ProgressPercentage - 202 + "%";
                totalProgress.Value = e.ProgressPercentage - 202;
            }
            else if (e.ProgressPercentage == -1)
            {
                MessageBox.Show(e.UserState as string);
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            a3UpdateCheck();
            indivProgress.Value = 0;
            midProgress.Value = 0;
            totalProgress.Value = 0;
            midProgressTxt.Content = "";
            indivProgressTxt.Content = "";
            totalProgressTxt.Content = "";
            _downloadProgress = "";
            dlSpeedTimer.Stop();
            updateTitle();
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.IsEnabled = false;
            var abt = new About();
            abt.Show();
        }

        private void logging_click(object sender, RoutedEventArgs e)
        {
            var logging = new log();
            logging.Show();
        }

        private void repoHide()
        {
            repoGen.Visibility = Visibility.Hidden;
        }

        private void showrepo(object sender, MouseButtonEventArgs e)
        {
            clickCount++;
            if (clickCount > 4)
            {
                repoGen.Visibility = Visibility.Visible;
            }
        }

        private void LaunchAndJoin(object sender, RoutedEventArgs e)
        {
            if (CurrentGame == "Arma 2")
            {
                string gameversion = Settings.Default.gameversion;
                string server = Slickversion.repos[repomenu.SelectedIndex].server;
                string password = Slickversion.repos[repomenu.SelectedIndex].password;
                Launch.a2Launch(true, server, password);
            }
            else if (CurrentGame == "Arma 3")
            {
                string gameversion = Settings.Default.gameversion;
                string server = Slickversion.repos[repomenu.SelectedIndex].server;
                string password = Slickversion.repos[repomenu.SelectedIndex].password;
                Launch.a3Launch(true, server, password);
            }
        }

        private void initRepos()
        {
            //List<ComboBoxItem> repos = new List<ComboBoxItem>();
            if (Settings.Default.A3repo != "")
            {
                repomenu.SelectedIndex = int.Parse(Settings.Default.A3repo);
            }

            foreach (Repos repo in Slickversion.repos)
            {
                string Game = "";
                if (repo.game == "arma2")
                {
                    Game = "Arma 2";
                }
                else if (repo.game == "arma3")
                {
                    Game = "Arma 3";
                }
                var newItem = new ComboBoxItem();
                newItem.Tag = repo.url;
                newItem.Content = Game + " | " + repo.name;
                newItem.MouseDown += setActiveRepo;
                repomenu.Items.Add(newItem);
            }
        }

        private void setActiveRepo(object sender, RoutedEventArgs e)
        {
            if (Slickversion.repos[repomenu.SelectedIndex].url == "not")
            {
                MessageBox.Show("This repo has not yet been implemented. Setting you to default");
                repomenu.SelectedIndex = 0;
                Settings.Default.A3repo = "" + 0;
                Settings.Default.A3repourl = Slickversion.repos[0].url;
            }
            else
            {
                Settings.Default.A3repo = "" + repomenu.SelectedIndex;
                Settings.Default.A3repourl = Slickversion.repos[repomenu.SelectedIndex].url;
            }
            
            if (repomenu.IsDropDownOpen)
            {
                a3UpdateCheck();
            }
           logocheck();
           InitProperties();
        }

        private void refreshEvents(object sender, RoutedEventArgs e)
        {
            eventbox.Items.Clear();
            _rposts.Clear();
            RedditWorker.RunWorkerAsync();
            eventbutton.IsEnabled = false;
        }

        //logo change
        private void logocheck()
        {
            String currentGame = String.Empty;
            if ((repomenu.SelectedIndex) < (Slickversion.repos.Count))
            {
                Repos currentRepo = Slickversion.repos[repomenu.SelectedIndex];
                currentGame = currentRepo.game;
            } 

            if (currentGame == "arma2")
            {
                logo_image.Source = new BitmapImage(new Uri(@"Resources/ArmA2.png", UriKind.Relative));
                mainTab.Header = "Arma 2";
                CurrentGame = "Arma 2";
                UpdateManager.isArma2 = true;
            }
            else
            {
                logo_image.Source = new BitmapImage(new Uri(@"Resources/ArmA3.png", UriKind.Relative));
                mainTab.Header = "Arma 3";
                CurrentGame = "Arma 3";
                UpdateManager.isArma2 = false;
            }
        }

        private void redditWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = @"http://www.reddit.com" + _subreddit + "/hot.json";
            string json = String.Empty;
            try
            { 
                json = downloader.webRead(url);
            }
            catch(Exception ex)
            {
                logIt.add(ex.ToString());
                return;
            }

            var topic = JsonConvert.DeserializeObject<RootObject>(json);

            foreach (Child i in topic.data.children)
            {
                if (i.data.link_flair_text == "EVENT")
                {
                    var evt = new events {title = i.data.title, author = i.data.author, url = i.data.permalink};
                    _rposts.Add(evt);
                }
            }
        }

        private void redditworker_Done(object sender, AsyncCompletedEventArgs e)
        {
            foreach (events evn in _rposts)
            {
                var newEvent = new Button
                {
                    Content = evn.title + " by " + evn.author,
                    Height = 50,
                    Width = 520,
                    Tag = evn.url,
                    FontSize = 14
                };
                newEvent.Click += newEvent_Click;
                eventbox.Items.Add(newEvent);
            }
            eventbutton.IsEnabled = true;
        }

        private void newEvent_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Process.Start("http://www.reddit.com" + button.Tag);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void a2DirText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.A2path = a2DirText.Text;
            Settings.Default.Save();
        }

        private void va2DirText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.vA2Path = va2DirText.Text;
            Settings.Default.Save();
        }

        #region TOPSECRET EASTEREGG NO PEAKING

        private int nyanClick;

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

        public void updateGuides(object sender, RoutedEventArgs e)
        {
            Guidebox.Items.Clear();            
            List<Link> _links = new List<Link>(); 
            string jsonString = downloader.webRead("http://arma.projectawesome.net/beta/repo/slickupdater/guides.json");
            var guides = JsonConvert.DeserializeObject<Guide>(jsonString);
            _links = guides.Links;
            foreach (var guide in _links)
            {
                var image = new Image()
                {
                    Source = new BitmapImage(new Uri(guide.icon)),
                    Width = 42,
                    Height = 42,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(4, 0, 0, 0)
                };

                var newGuide = new Button
                {
                    Content = "                  " + guide.title,
                    Height = 50,
                    Width = 521,
                    Tag = guide.url + "",
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };

                var panel = new Grid();
                panel.Children.Add(newGuide);
                panel.Children.Add(image);

                newGuide.Click += GuideClick;
                Guidebox.Items.Add(panel);
            }
        }

        private static void GuideClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Process.Start(button.Tag + "");
        }
    }

    public class Mod
    {
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