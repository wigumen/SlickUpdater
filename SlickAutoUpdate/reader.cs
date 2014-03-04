using System;
using System.Net;
using System.IO;

namespace SlickAutoUpdate
{
    class reader
    {
        public static string webRead(string url)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(url);
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();
            return content;
        }
    }
}
