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
using System.ComponentModel;

namespace SlickUpdater
{
    /// <summary>
    /// Interaction logic for log.xaml
    /// </summary>
    public partial class log : Window
    {
        BackgroundWorker autoUpdate = new BackgroundWorker();

        public log()
        {
            InitializeComponent();
            //logwindow.Text = logIt.logData;
            autoUpdate.DoWork += constantUpdate;
            autoUpdate.RunWorkerCompleted += constantUpdateDone;
            autoUpdate.RunWorkerAsync();
            update();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            logIt.logData = "";
            update();
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(logIt.logData);
        }

        private void updatelog_Click(object sender, RoutedEventArgs e)
        {
            update();
        }

        public void update()
        {
            logwindow.Text = logIt.logData;
        }

        public void constantUpdate(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(200);
        }

        private void constantUpdateDone(object sender, RunWorkerCompletedEventArgs e)
        {
            update();
            autoUpdate.RunWorkerAsync();
        }
    }
}
