using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using IdentityParser;
using Newtonsoft.Json.Linq;
using ServiceHandler;


namespace CNT5517_Project
{
    class SocketObj
    {
        private int port;
        private Socket socket;
        private IPAddress Host;
        private IPEndPoint ipe;
        private bool connected = false;
        private bool keepListening = false;
        private bool destroyListenSocket = false;

        public void ServerSocketObj(){}

        /*
        public void ServerSocketObj(string _host, int _port)
        {
            Host = IPAddress.Parse(_host);
            port = _port;
            ipe = new IPEndPoint(Host, port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
        */
        //public void ConnectSocket(){ socket.Connect(ipe);}

        private void SetConnectionStatus() 
        {
            if(socket.Connected)
            {
                connected = true;
            }
        }

       

        private void CloseSocket()
        {
            try{socket.Shutdown(SocketShutdown.Both);}
            finally{socket.Close();}
        }

        public bool GetConnectionStatus(){return connected;}

        public void SetListeningStatus(bool status)
        {
            keepListening = status;
        }

        public void DestroyListener(bool status)
        {
            destroyListenSocket = status;
        }

        /* move to program.cs
        public string SendServiceCallTweets(string tweet, string ipAddr, int _port)
        {
            Host = IPAddress.Parse(ipAddr);
            port = _port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ipe = new IPEndPoint(IPAddress.Parse(ipAddr), port);

            try
            {
                socket.Connect(ipe);
                SetConnectionStatus();
                socket.Send(Encoding.ASCII.GetBytes(tweet));
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to send Service Call Tweet to " + ipAddr);
                return null;
            }
            

            byte[] response = new byte[1024];
            if (socket.Receive(response) != 0)
            {
                string str = Encoding.ASCII.GetString(response, 0, response.Length);

                //Console.WriteLine("Response from " + ipAddr + ":\n" + str);
                CloseSocket();
                return str;
                //socket.Close();
            }
            else
            {
                Console.WriteLine("Cannot receive reply tweet");
                return null;
            }



        }
        */

        /* receive Tweets from a multicast address method */
        public void ListenForTweets(Identity_Parser IDP, Service_Handler SVH)
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

            //Console.WriteLine("Start listening to Tweets at multicast address: " + multicastAddr + ":" + multicastPort);
            while (true)
            {

                if (destroyListenSocket)
                {
                    Console.WriteLine("Leave the multicast group1: " + multicastAddr + ":" + multicastPort);
                    sock.Close();
                    break;
                }
                /* Deal with received Tweets */

                byte[] buffer = new byte[1024];
                //if (sock.Receive(buffer) != 0)
                if (sock.Available != 0)
                {
                    sock.Receive(buffer);
                    string tweet = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

                    /* The socket is still there, but if user stop listening, we don't store the received Tweets */
                    if (keepListening)
                    {
                        Console.WriteLine("\n" + tweet);

                        JObject jsonOBJ = JObject.Parse(tweet);
                        string tweetType = (string)jsonOBJ["Tweet Type"];

                        if (tweetType == "Identity_Thing")
                        {
                            Console.WriteLine("Tweet type saved:" + (string)jsonOBJ["Tweet Type"]);
                            IDP.parse_IdentityTweets(tweet);
                        }
                        else if (tweetType == "Identity_Language")
                        {
                            IDP.parse_LanguageTweets(tweet);

                        }
                        else if (tweetType == "Identity_Entity")
                        {
                            IDP.parse_EntityTweets(tweet);
                        }
                        else if (tweetType == "Service")
                        {
                            SVH.parse_ServiceTweets(tweet);
                            //TODO: Service Tweets
                        }
                        else if (tweetType == "Relationship")
                        {
                            SVH.parse_RelationTweets(tweet);
                        }
                        else
                        {
                            Console.WriteLine("Received unhandle tweet types\n");
                            //Unhandled Tweet Types, might be unbounded tweets?
                        }

                    }

                    if (destroyListenSocket)
                    {
                        Console.WriteLine("Leave the multicast group2: " + multicastAddr + ":" + multicastPort);
                        sock.Close();
                        break;
                    }
                }
                

            }
        }

        
        public void ListenForTweets_TCP(Identity_Parser IDP, Service_Handler SVH)
        {
            string fixaddrtoRPI = "10.254.254.64";
            Host = IPAddress.Parse(fixaddrtoRPI);
            port = 6667;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ipe = new IPEndPoint(IPAddress.Parse(fixaddrtoRPI), port);

            try
            {
                socket.Connect(ipe);
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable establich TCP connection " + fixaddrtoRPI);
                return;
            }



            
            Console.WriteLine("Start listening to Tweets at multicast address: " + fixaddrtoRPI + ":6667");
            while (true)
            {

                if (destroyListenSocket)
                {
                    Console.WriteLine("Leave the multicast group1: " + fixaddrtoRPI + ":6667");
                    socket.Close();
                    break;
                }
                /* Deal with received Tweets */

                byte[] buffer = new byte[1024];
                //if (sock.Receive(buffer) != 0)
                if (socket.Available != 0)
                {
                    socket.Receive(buffer);
                    string tweet = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

                    /* The socket is still there, but if user stop listening, we don't store the received Tweets */
                    if (keepListening)
                    {
                        Console.WriteLine("\n" + tweet);

                        JObject jsonOBJ = JObject.Parse(tweet);
                        string tweetType = (string)jsonOBJ["Tweet Type"];

                        if (tweetType == "Identity_Thing")
                        {
                            Console.WriteLine("Tweet type saved:" + (string)jsonOBJ["Tweet Type"]);
                            IDP.parse_IdentityTweets(tweet);
                        }
                        else if (tweetType == "Identity_Language")
                        {
                            IDP.parse_LanguageTweets(tweet);

                        }
                        else if (tweetType == "Identity_Entity")
                        {
                            IDP.parse_EntityTweets(tweet);
                        }
                        else if (tweetType == "Service")
                        {
                            SVH.parse_ServiceTweets(tweet);
                            //TODO: Service Tweets
                        }
                        else if (tweetType == "Relationship")
                        {
                            //TODO: Relationship Tweets
                        }
                        else
                        {
                            Console.WriteLine("Received unhandle tweet types\n");
                            //Unhandled Tweet Types, might be unbounded tweets?
                        }

                    }

                    if (destroyListenSocket)
                    {
                        Console.WriteLine("Leave the multicast group2: " + fixaddrtoRPI + ":6667");
                        socket.Close();
                        break;
                    }
                }


            }
        }


    }
}
