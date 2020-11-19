using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace IoTIDE
{
    class Program
    {

        /* Try to receive Tweets from a multicast address */
        static void ListenForTweets()
        {
            /* Multicast Address and Port which Atlas is using */
            int multicastPort = 1235;
            string multicastAddr = "232.1.1.1";

            /* Create a unicast UDP socket (multicast uses UDP) */
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            /* Create an endpoint to listen to, listen to a port on all interfaces */
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, multicastPort);
            sock.Bind(ipep);

            /* Add the socket to the multicast group */
            IPAddress ip = IPAddress.Parse(multicastAddr);
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
            Console.WriteLine("Start listening to Tweets at multicast address: " + multicastAddr + "/" + multicastPort);
            while (true)
            {
                /* Deal with received Tweets */
                byte[] buffer = new byte[1024];
                if (sock.Receive(buffer) != 0)
                {
                    string str = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                    Console.WriteLine(str.Trim());
                    //TODO: Sotre the recevied Tweet in data structures
                }
            }
        }

        static void SendServiceCallTweets(string tweet, string ipAddr, int port)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAddr), port);

            try
            {
                server.Connect(ipep);
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to send Service Call Tweet to " + ipAddr);
                return;
            }
            
            server.Send(Encoding.ASCII.GetBytes(tweet));
            
            while (true)
            {
                byte[] response = new byte[1024];
                if (server.Receive(response) != 0)
                {
                    string str = Encoding.ASCII.GetString(response, 0, response.Length);
                    Console.WriteLine("Response from " + ipAddr + ":\n" + str);
                    server.Close();
                    return;
                }
            }

        }

        static void Main(string[] args)
        {
            /* Listen to Tweets in the background as a thread */
            Thread tweetListener = new Thread(new ThreadStart(ListenForTweets));
            tweetListener.IsBackground = true;
            tweetListener.Start();

            string tweet = "{\"Tweet Type\":\"Service call\",\"Thing ID\":\"MyRPI_5341\"," +
                "\"Space ID\":\"MySmartSpace\",\"Service Name\":\"RedLEDBlink\",\"Service Inputs\":\"(5)\"}";
            string ipAddr = "192.168.3.55";
            int port = 6668;
            
            


            Console.WriteLine("Press <enter> to send Service Call Tweet");
            Console.ReadLine();
            SendServiceCallTweets(tweet, ipAddr, port);
            Console.WriteLine("Press <enter> to quit");
            Console.ReadLine();
        }
    }
}
