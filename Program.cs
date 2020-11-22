using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using CNT5517_Project;
using IdentityParser;
using ServiceHandler;
using ServiceParser;


namespace IoTIDE
{
    class Program
    {

        private Identity_Parser IDP = new Identity_Parser();
        private Service_Handler SVH = new Service_Handler();

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
                server.Send(Encoding.ASCII.GetBytes(tweet));
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to send Service Call Tweet to " + ipAddr);
                return;
            }
            
            
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
                    tweetListenerThread = new Thread(() => sockObj.ListenForTweets(IDP, SVH));
                    //tweetListenerThread = new Thread(() => sockObj.ListenForTweets_TCP(IDP));
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

            /* display service tweets brief information*/
            foreach (KeyValuePair<string, Dictionary<string, Tservice>> entry in SVH.thingServiceTweets)
            {
                Console.WriteLine("Thing ID:" + entry.Key);
                foreach (KeyValuePair<string, Tservice> entry2 in entry.Value)
                {
                    Console.Write(" EID: " + entry2.Value.entityID + "  ");
                    Console.Write(" ServiceName: " + entry2.Value.serviceName);
                    Console.WriteLine("\n");
                }

            }

            /* display relation tweets brief information*/
            foreach (KeyValuePair<string, Dictionary<string, TRelation>> entry in SVH.thingRelationships)
            {
                Console.WriteLine("Thing ID:" + entry.Key);
                foreach (KeyValuePair<string, TRelation> entry2 in entry.Value)
                {
                    //Console.Write(" EID: " + entry2.Value.entityID + "  ");
                    //Console.Write(" ServiceName: " + entry2.Value.serviceName);
                    entry2.Value.displayInfo();
                    Console.WriteLine("\n");
                }

            }
        }

        private void showServicesAll()
        {
            /* display all services */
            foreach (KeyValuePair<string, Dictionary<string, Tservice>> entry in SVH.thingServiceTweets)
            {
                Console.WriteLine("Thing ID:" + entry.Key);
                foreach (KeyValuePair<string, Tservice> entry2 in entry.Value)
                {
                    Console.Write(" EID: " + entry2.Value.entityID + "  ");
                    Console.Write(" ServiceName: " + entry2.Value.serviceName);
                    Console.WriteLine("\n");
                }

            }
        }

        static void Main(string[] args)
        {
            /* Listen to Tweets in the background as a thread */
            //Thread tweetListener = new Thread(new ThreadStart(ListenForTweets));
            //tweetListener.IsBackground = true;
            //tweetListener.Start();


            /*
            string inputAPI0 = "ServiceName:[NULL]:(NULL)";
            string inputAPI1 = "ServiceName:[input1,int,NULL]:(NULL)";
            string inputAPI2 = "ServiceName:[input1,int,input2,int,NULL]:(NULL)";
            string inputAPI3 = "ServiceName:[time,int, NULL|time2,int, NULL]:(value,int, NULL)";
            string inputAPI4 = "ServiceName:[input1,int,input2,int,NULL]:(output1,int,NULL)";
            string[] IOstrDelimiter = {":[", "]:(", ":(", ")" };
                
            string[] IOstr = inputAPI3.Split(IOstrDelimiter, StringSplitOptions.RemoveEmptyEntries);
            string inputFormat = IOstr[1].Replace(" ","");
            string outputFormat = IOstr[2].Replace(" ", "");
            Console.WriteLine("InputFormat: \"{0}\" OutputFormat: \"{1}\"", inputFormat, outputFormat);

            char[] IOwordsDelimiter = {',', '|'};
            string[] inputWords = inputFormat.Split(IOwordsDelimiter);
            string[] ouputWords = outputFormat.Split(IOwordsDelimiter);

            foreach (var word in inputWords)
            {
                Console.WriteLine("{0}", word);
            }
            */

            Program pp = new Program();
            string inputStr;
            
            while (true)
            {
                Console.WriteLine("\n\nCommands: connect disconnect pause resume send showall showservice show relation\n\n");
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
                {
                    pp.printTweetsTest();
                    Console.WriteLine("Enter ThingID and ServiceName: {ThingID ServiceName}");
                    string str = Console.ReadLine();
                    string[] words = str.Split(' ');
                    if (words.Length != 2)
                        continue;
                    string thingID = words[0];
                    string serviceName = words[1];
                    string tweetServiceCall = pp.SVH.genServiceCallTweet(thingID, serviceName);

                    if (tweetServiceCall != null)
                    {
                        if (!pp.IDP.thingLanguageTweets.ContainsKey(thingID))
                            Console.WriteLine("Needs to receive {0}'s Identity_Language tweet first, pls listen for more tweets...", thingID);
                        string ipaddress = pp.IDP.thingLanguageTweets[thingID].thingIP;
                        pp.SendServiceCallTweets(tweetServiceCall, ipaddress, 6668);
                    }

                }
                else if (inputStr == "showall")
                {
                    pp.printTweetsTest();
                }
                else if (inputStr == "showservice")
                {
                    Console.WriteLine("Enter ThingID and ServiceName: {ThingID ServiceName}");
                    string str = Console.ReadLine();
                    string[] words = str.Split(' ');
                    if (words.Length != 2)
                        continue;
                    pp.SVH.showServiceAPI(words[0], words[1]);
                }
                else if (inputStr == "showrelation")
                {
                    Console.WriteLine("Enter ThingID and RelationName: {ThingID RelationName}");
                    string str = Console.ReadLine();
                    string[] words = str.Split(' ');
                    if (words.Length != 2)
                        continue;
                    pp.SVH.showRelationships(words[0], words[1]);
                }
                else if (inputStr == "recipe")
                {

                }
                else
                {
                    continue;
                }
                    
            }
        }
    }
}
