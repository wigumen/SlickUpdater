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
using System.Windows.Threading;

namespace SlickUpdater
{
    /// <summary>
    /// Interaction logic for time.xaml
    /// </summary>
    public partial class time : Window
    {
        public time()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
               this.dateText.Text = Convert.ToString(ntp.GetTime());
            }, this.Dispatcher);
        }
    }
}
