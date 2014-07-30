using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlickUpdater
{
        public class Repos
        {
            public string name { get; set; }
            public string url { get; set; }
            public string server { get; set; }
            public string joinText { get; set; }
            public string subreddit { get; set; }
            public string password { get; set; }
            public string game { get; set; }
        }

        public class versionfile
        {
            public string version { get; set; }
            public string download { get; set; }
            public List<Repos> repos { get; set; }
            public versionfile()
            {
                version = String.Empty;
                download = String.Empty;
                repos = new List<Repos>();
            }
        }

        public class Link
        {
            public string title { get; set; }
            public string url { get; set; }
            public string icon { get; set; }
        }

        public class Guide
        {
            public List<Link> Links { get; set; }
        }
    }
