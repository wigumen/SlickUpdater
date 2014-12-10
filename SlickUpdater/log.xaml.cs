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
    /// Interaction logic for log.xaml
    /// </summary>
    public partial class log : Window
    {
        public log()
        {
            InitializeComponent();
            //logwindow.Text = logIt.logData;
            update();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            logit.logData = "";
            update();
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(logit.logData);
        }

        private void updatelog_Click(object sender, RoutedEventArgs e)
        {
            update();
        }

        private void update()
        {
            logwindow.Text = logit.logData;
        }
    }
}
