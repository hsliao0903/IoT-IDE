using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace IoTIDE
{
    class Program
    {

        static void ListenForTweets()
        {
            /* Multicast Address and Port which Atlas is using */
            int multicastPort = 1235;
            string multicastAddr = "232.1.1.1";

            /* Create a unicast UDP socket (multicast uses UDP) */
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            /* Create an endpoint to listen to, listen to a port on all interfaces */
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, multicastPort);
            sck.Bind(ipep);

            /* Add the socket to the multicast group */
            IPAddress ip = IPAddress.Parse(multicastAddr);
            sck.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
            Console.WriteLine("Start listening to Tweets at multicast address: " + multicastAddr + "/" + multicastPort);
            while (true)
            {
                /* Deal with received Tweets */
                byte[] buffer = new byte[1024];
                if (sck.Receive(buffer) != 0)
                {
                    string str = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                    Console.WriteLine(str.Trim());
                    //TODO: Sotre the recevied Tweet in data structures
                }
            }
        }

        static void Main(string[] args)
        {
            Thread tweetListener = new Thread(new ThreadStart(ListenForTweets));
            tweetListener.IsBackground = true;
            tweetListener.Start();

            Console.WriteLine("Press <enter> to quit");
            Console.ReadLine();

        }
    }
}
