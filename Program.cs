using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using CNT5517_Project;
using IdentityParser;

using System.Collections.Generic;


namespace IoTIDE
{
    class Program
    {

        private Identity_Parser IDP = new Identity_Parser();
        private SocketObj sockObj = new SocketObj();
        private Thread tweetListenerThread;
        private bool listenerAlive = false;
        
        
        private void SendServiceCallTweets(string tweet, string ipAddr, int port)
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



        //private void connectBtn_Click(object sender, EventArgs e)
        private void connectBtn_Click()
        {
            //connectBtn.Enabled = false;
            //connectBtn.Text = "Start"
            //serverSocket = new SocketObj(hostTextBox.Text, int.Parse(portTextBox.Text));
            try
            {
                if (!listenerAlive)
                {
                    //connectBtn.Enabled = false;
                    sockObj.DestroyListener(false);
                    sockObj.SetListeningStatus(true);
                    tweetListenerThread = new Thread(() => sockObj.ListenForTweets(IDP));
                    tweetListenerThread.IsBackground = true;
                    tweetListenerThread.Start();

                    Console.WriteLine("Joining multicast address group 232.1.1.1/1235 ");
                    /* pause for connection estalishing */
                    Thread.Sleep(1000);



                    Console.WriteLine("Successfully listening to multicast address group 232.1.1.1/1235");
                    listenerAlive = true;
                    //logTextBox.AppendText("Successfully connected to: " + hostTextBox.Text);
                    //logTextBox.AppendText(Environment.NewLine);

                    //logTextBox.AppendText("Establishing connection to multicast address 231.1.1.1/1235 ");
                    //logTextBox.AppendText(Environment.NewLine);
                }
            }
            catch
            {
                Console.WriteLine("Socket failed to join multicast address group 232.1.1.1/1235 ");
                //logTextBox.AppendText("Connection failed to: " + hostTextBox.Text);
                //logTextBox.AppendText(Environment.NewLine);
                //connectBtn.Enabled = true;
                //connectBtn.Focus();
                return;
            }




        }

        private void listeningBtn(bool listen_status)
        {
            if (!listen_status)
            {
                Console.WriteLine("Pause to receive Tweets");
                sockObj.SetListeningStatus(false);
            }
            else
            {
                
                Console.WriteLine("Resume to receive Tweets");
                sockObj.SetListeningStatus(true);
            }
        }

        private void destroyListener()
        {
            if (listenerAlive)
            {
                sockObj.DestroyListener(true);
                listenerAlive = false;
            }
        }

        private void printTweetsTest()
        {
            Console.WriteLine("\nOutput received tweet:");
            IDP.display_IdentityTweets();
            Console.WriteLine();
            IDP.display_LanguageTweets();
            Console.WriteLine();
            IDP.display_EntityTweets();
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            /* Listen to Tweets in the background as a thread */
            //Thread tweetListener = new Thread(new ThreadStart(ListenForTweets));
            //tweetListener.IsBackground = true;
            //tweetListener.Start();

            string tweet = "{\"Tweet Type\":\"Service call\",\"Thing ID\":\"MyRPI_5341\"," +
                "\"Space ID\":\"MySmartSpace\",\"Service Name\":\"RedLEDBlink\",\"Service Inputs\":\"(5)\"}";
            string tweet2 = "{\"Tweet Type\":\"Service call\",\"Thing ID\":\"MyRPI_5341\"," +
                "\"Space ID\":\"MySmartSpace\",\"Service Name\":\"RedLEDBlink\",\"Service Inputs\":\"\"}";
            //string tweet3 = "{ \"Tweet Type\" : \"Service\",\"Name\" : \"RedLEDBlink\",\"Thing ID\" : \"MyRPI_5341\",\"Entity ID\" : \"Entity_1\",\"Space ID\" : \"MySmartSpace\",\"Vendor\" : \"\",\"API\" : \"RedLEDBlink:[\"times\",int, NULL]:(NULL)\",\"Type\" : \"Action\",\"AppCategory\" : \"Lighting\",\"Description\" : \"Blink the Red LED for an amount of times.\",\"Keywords\" : \"LED,BLINK,Times\" }";
            string ipAddr = "10.254.254.64";
            int port = 6668;

            Program pp = new Program();
            string inputStr;
            JObject rss = JObject.Parse(tweet2);
            string test = (string)rss["API"];

            Console.WriteLine("testttt " + test);




            while (true)
            {
                inputStr = Console.ReadLine();
                if (inputStr == "connect")
                    pp.connectBtn_Click();
                else if (inputStr == "pause")
                    pp.listeningBtn(false);
                else if (inputStr == "resume")
                    pp.listeningBtn(true);
                else if (inputStr == "disconnect")
                    pp.destroyListener();
                else if (inputStr == "send")
                    pp.SendServiceCallTweets(tweet, ipAddr, port);
                else if (inputStr == "test")
                {
                    pp.printTweetsTest();

                }
                else
                    continue;

            }


            //return;
            //Console.WriteLine("Press <enter> to send Service Call Tweet");
            //Console.ReadLine();
            //SendServiceCallTweets(tweet, ipAddr, port);
            //Console.WriteLine("Press <enter> to quit");
            //Console.ReadLine();
        }
    }
}
