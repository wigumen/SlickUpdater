using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlickAutoUpdate
{
    public class Repos
    {
        public string name { get; set; }
        public string url { get; set; }
        public string server { get; set; }
        public string joinText { get; set; }
        public string subreddit { get; set; }
    }

    public class versionfile
    {
        public string version { get; set; }
        public string download { get; set; }
        public List<Repos> repos { get; set; }
    }
}
