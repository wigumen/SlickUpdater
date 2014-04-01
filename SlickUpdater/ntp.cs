using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SlickUpdater
{
    class ntp
    {
        public static DateTime GetTime()
        {
            while (true)
            {
                try
                {
                    // NTP message size - 16 bytes of the digest (RFC 2030)
                    var ntpData = new byte[48];

                    //Setting the Leap Indicator, Version Number and Mode values
                    ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)



                    IPAddress ip = getServer();

                    //The UDP port number assigned to NTP is 123
                    var ipEndPoint = new IPEndPoint(ip, 123);
                    //NTP uses UDP
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    socket.Connect(ipEndPoint);

                    //Stops code hang if NTP is blocked
                    socket.ReceiveTimeout = 3000;

                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    socket.Close();

                    //Offset to get to the "Transmit Timestamp" field (time at which the reply 
                    //departed the server for the client, in 64-bit timestamp format."
                    const byte serverReplyTime = 40;

                    //Get the seconds part
                    ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                    //Get the seconds fraction
                    ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                    //Convert From big-endian to little-endian
                    intPart = SwapEndianness(intPart);
                    fractPart = SwapEndianness(fractPart);

                    var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                    //**UTC** time
                    var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

                    return networkDateTime;
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    throw new NoServerFoundException("Connection failed!");
                }
            }
        }

        // Endian Bit Conversion
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
        public static string[] srvs = {
            "time.nist.gov",
            "0.pool.ntp.org",
            "0.europe.pool.ntp.org",
            "0.north-america.pool.ntp.org",
            "pool.ntp.org",
            "north-america.pool.ntp.org",
            "europe.pool.ntp.org",
             };
        private static uint lastSrv;
        public static IPAddress getServer()
        {
            lastSrv = (uint)((lastSrv + 1) % srvs.Length);
            IPAddress[] address = Dns.GetHostEntry(srvs[lastSrv]).AddressList;
            if (address == null || address.Length == 0)
                throw new NoServerFoundException("No IP found");
            return address[0];
        }

        public class NoServerFoundException : System.Exception {
        public NoServerFoundException() : base() { }
        public NoServerFoundException(string message) : base(message) { }
        public NoServerFoundException(string message,
                System.Exception inner) : base(message, inner) { }
        protected NoServerFoundException(SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
        }
    }
}
