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
        private List<APP_Handler> _APP = new List<APP_Handler>();

        private SocketObj sockObj = new SocketObj();
        private Thread tweetListenerThread;
        private bool listenerAlive = false;
        
        
        private string SendServiceCallTweets(string tweet, string ipAddr, int _port)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAddr), _port);

            try
            {
                server.Connect(ipep);
                server.Send(Encoding.ASCII.GetBytes(tweet));
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to send Service Call Tweet to " + ipAddr);
                return null;
            }


            byte[] response = new byte[1024];
            if (server.Receive(response) != 0)
            {
                string str = Encoding.ASCII.GetString(response, 0, response.Length);

                //Console.WriteLine("Response from " + ipAddr + ":\n" + str);
                server.Close();
                return str;
            }
            else
            {
                Console.WriteLine("Cannot receive reply tweet");
                return null;
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

                    Console.WriteLine("Joining multicast address group 232.1.1.1:1235 ");
                    /* pause for connection estalishing */
                    Thread.Sleep(1000);



                    Console.WriteLine("Successfully listening to multicast address group 232.1.1.1:1235");
                    listenerAlive = true;
                    //logTextBox.AppendText("Successfully connected to: " + hostTextBox.Text);
                    //logTextBox.AppendText(Environment.NewLine);

                    //logTextBox.AppendText("Establishing connection to multicast address 231.1.1.1/1235 ");
                    //logTextBox.AppendText(Environment.NewLine);
                }
            }
            catch
            {
                Console.WriteLine("Socket failed to join multicast address group 232.1.1.1:1235 ");
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
            SVH.display_ServiceTweets();


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
                Console.WriteLine("TID ServiceName Pairs:");
                foreach (KeyValuePair<string, Tservice> entry2 in entry.Value)
                {
                    Console.WriteLine(entry2.Value.thingID + " " + entry2.Value.serviceName);
                }

            }
        }

        private APP_Handler getAPP(List<APP_Handler> appStruct ,string appName)
        {
            foreach (var entry in appStruct)
            {
                if (entry.appName == appName)
                {
                    return entry;
                    
                }

            }
            return null;
        }

        private bool isServiceSucceed(int sID, APP_Handler curAPP, Reply_Info reply)
        {
            if (sID == 1)
            {
                if ((curAPP.hasOutputSC1 && curAPP.expectResult1 == reply.serviceResult)
                         || (!curAPP.hasOutputSC1 && reply.status == "Successful"))
                    return true;
                
                else
                    return false;
            }
            else
            {
                if ((curAPP.hasOutputSC2 && curAPP.expectResult2 == reply.serviceResult)
                                || (!curAPP.hasOutputSC2 && reply.status == "Successful"))
                    return true;
                else
                    return false;
            }
           
        }



        private int activateAPP(APP_Handler curAPP)
        {
            if (curAPP.numService == 1)
            {
                string replyTweet = SendServiceCallTweets(curAPP.tweetSC1, curAPP.ipAddrSC1, curAPP.port1);
                Reply_Info reply = SVH.parse_ServiceReplyTweets(replyTweet);
                showReply(reply);
                //Console.WriteLine("Reply tweet:\n{0}", replyTweet);
                return 0;
            }
            else if (curAPP.numService == 2)
            {
                // deal with each relation between these two services
                string replyTweet = null;
                Reply_Info reply = null;
                switch (curAPP.relation)
                {
                    // IF A then B , USE A to do B , they all need to make sure service A is successfully activated first then do B
                    case "Control": 
                    case "Drive":
                        // execute service A first
                        replyTweet = SendServiceCallTweets(curAPP.tweetSC1, curAPP.ipAddrSC1, curAPP.port1);
                        reply = SVH.parse_ServiceReplyTweets(replyTweet);

                        // check the IF predicate for service A, is the reply expected?
                        if (isServiceSucceed(1, curAPP, reply))
                        {
                            showReply(reply);
                            // true do service B
                            replyTweet = SendServiceCallTweets(curAPP.tweetSC2, curAPP.ipAddrSC2, curAPP.port2);
                            reply = SVH.parse_ServiceReplyTweets(replyTweet);
                            if (isServiceSucceed(2, curAPP, reply))
                            {
                                // service B succeed
                                showReply(reply);
                                Console.WriteLine("\nAPP \"{0}\" successfully activated!", curAPP.appName);
                                return 0;
                            }
                            else
                            {
                                showReply(reply);
                                Console.WriteLine("\nAPP \"{0}\" failed activated because \"{1}\" failed", curAPP.appName, curAPP.SPI2);
                                return -1;
                            }
                        }
                        else
                        {
                            // false do not do service B
                            showReply(reply);
                            Console.WriteLine("\nAPP \"{0}\" failed activated because \"{1}\" failed", curAPP.appName, curAPP.SPI1);
                        }
                        break;

                    // Check B then do A , while doing B Do A , they all need to make sure service B is successfully activated first then do A
                    // just like the Control and Drive relation, but swap the order of service A and B
                    case "Support":
                    case "Extend":
                        // execute service B first
                        replyTweet = SendServiceCallTweets(curAPP.tweetSC2, curAPP.ipAddrSC2, curAPP.port2);
                        reply = SVH.parse_ServiceReplyTweets(replyTweet);

                        // check the IF predicate for service B, is the reply expected?
                        if (isServiceSucceed(2, curAPP, reply))
                        {
                            showReply(reply);
                            // if true do service A
                            replyTweet = SendServiceCallTweets(curAPP.tweetSC1, curAPP.ipAddrSC1, curAPP.port1);
                            reply = SVH.parse_ServiceReplyTweets(replyTweet);
                            if (isServiceSucceed(1, curAPP, reply))
                            {
                                // service A succeed
                                showReply(reply);
                                Console.WriteLine("\nAPP \"{0}\" successfully activated!", curAPP.appName);
                                return 0;
                            }
                            else
                            {
                                showReply(reply);
                                Console.WriteLine("\nAPP \"{0}\" failed activated because \"{1}\" failed", curAPP.appName, curAPP.SPI1);
                                return -1;
                            }
                        }
                        else
                        {
                            // if false do not do service A
                            showReply(reply);
                            Console.WriteLine("\nAPP \"{0}\" failed activated because \"{1}\" failed", curAPP.appName, curAPP.SPI2);
                        }
                        break;
                    case "Contest":
                        Console.WriteLine("\nAPP \"{0}\" failed activated because relation \"Contest\" not supported yet", curAPP.appName);
                        break;
                    case "Interfere":
                        Console.WriteLine("\nAPP \"{0}\" failed activated because relation \"Interfere\" not supported yet", curAPP.appName);
                        break;
                }

            }
            else
            {
                Console.WriteLine("Cannot activate the APP");
                return 0;
            }
            return -1;
        }

        private void showReply(Reply_Info reply)
        {
            Console.WriteLine("Reply for activated \"{0}\"", reply.serviceName);
            Console.WriteLine("\nStatus: {0}", reply.status);
            Console.WriteLine("Description: {0}", reply.statusDesc);
            if (reply.serviceResult == null)
                Console.WriteLine("Result Output: N/A");
            else
                Console.WriteLine("Result Output: {0}", reply.serviceResult);

        }

        static void Main(string[] args)
        {
            Console.WriteLine("\n\nCNT5517 - Group 4, IDE Project Console Version\n");

            Program pp = new Program();
            string inputStr;
            while (true)
            {
                
                Console.WriteLine("\n\nPlease enter the commands below:\n" +
                    "connect\t\t(Start receiving Tweets through multicast group 232.1.1.1:1235)\n" +
                    "disconnect\t(Stop receiving Tweets and leave multicast group)\n" +
                    "pause\t\t(Pause to receive Tweets)\n" +
                    "resume\t\t(Resume to receive Tweets)\n" +
                    "thing\t\t(Show \"Thing\" information briefly)\n" +
                    "entity\t\t(Show \"Entity\" information briefly)\n" +
                    "service\t\t(Show \"Service\" information briefly)\n" +
                    "showall\t\t(Show all received Tweets in a brief format, for debugging)\n" +
                    "recipe\t\t(Build an APP by selecting Services and adding relationship)\n" +
                    "app\t\t(Activate APPs that has been finalized by recipe cmd)\n" +
                    "exit\t\t(Terminate this IDE program)\n");

                inputStr = Console.ReadLine();
                if (inputStr == "connect")
                {
                    Console.WriteLine("\nExecute cmd connect\n");
                    pp.connectBtn_Click();
                }
                else if (inputStr == "pause")
                {
                    Console.WriteLine("\nExecute cmd pause\n");
                    pp.listeningBtn(false);
                }
                else if (inputStr == "resume")
                {
                    Console.WriteLine("\nExecute cmd resume\n");
                    pp.listeningBtn(true);
                }
                else if (inputStr == "disconnect")
                {
                    Console.WriteLine("\nExecute cmd disconnect\n");
                    pp.destroyListener();
                }
                else if (inputStr == "thing")
                {
                    Console.WriteLine("\nCommand thing:\n");
                    pp.IDP.display_Valid_Things();
                }
                else if (inputStr == "entity")
                {
                    Console.WriteLine("\nCommand entity:\n");
                    pp.IDP.display_EntityTweets();
                }
                else if (inputStr == "service")
                {
                    Console.WriteLine("\nCommand service:\n");
                    pp.SVH.display_ServiceTweets();
                }
                else if (inputStr == "send")
                {
                    pp.printTweetsTest();
                    Console.WriteLine("Enter ThingID and ServiceName: {ThingID ServiceName}");
                    string str = Console.ReadLine();
                    string[] words = str.Split(' ');
                    if (words.Length != 2) continue;
                    if (!pp.SVH.isServiceExist(words[0], words[1])) continue;

                    string thingID = words[0];
                    string serviceName = words[1];
                    string inputs = pp.SVH.getServiceInput(thingID, serviceName);
                    string tweetServiceCall = pp.SVH.genServiceCallTweet(thingID, serviceName, inputs);

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
                    pp.SVH.showServiceAPI(words[0], words[1], "both");
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
                    Console.WriteLine("You are able to choose 1 or 2 services listed below:");
                    pp.showServicesAll();
                    Console.WriteLine("How many services you would like to choose? (1/2)");
                    string nums = Console.ReadLine();
                    if (nums == "1")
                    {
                        Console.WriteLine("Enter ThingID and ServiceName pair: {ThingID ServiceName}");
                        string str = Console.ReadLine();
                        string[] words = str.Split(' ');
                        if (words.Length != 2) continue;
                        if (!pp.SVH.isServiceExist(words[0], words[1])) continue;

                        string thingID = words[0];
                        string serviceName = words[1];

                        /* Set the APP for finalization */
                        APP_Handler app = new APP_Handler();
                        app.numService = 1;
                        app.relation = null;
                        app.SPI1 = serviceName;
                        app.SPI2 = null;
                        string inputs = pp.SVH.getServiceInput(thingID, serviceName);
                        app.tweetSC1 = pp.SVH.genServiceCallTweet(thingID, serviceName, inputs);
                        app.tweetSC2 = null;
                        app.hasOutputSC1 = pp.SVH.hasOuput(thingID, serviceName);
                        app.hasOutputSC2 = false;
                        app.ipAddrSC1 = pp.IDP.thingLanguageTweets[thingID].thingIP;
                        app.ipAddrSC2 = null;
                        app.port1 = app.port2 = 6668;

                        /* TODO: maybe show the user what is the app setting right now */

                        Console.WriteLine("Enter a name for this APP or \"clear\" to give up:");
                        string userInput = Console.ReadLine();
                        if (userInput == "clear")
                        {
                            Console.WriteLine("Clear the recipe settings... Done");
                            continue;
                        }
                        app.appName = userInput;
                        pp._APP.Add(app);
                        Console.WriteLine("Your APP {0} has been finalized, please activate it thru app command", app.appName);

                    }
                    else if (nums == "2")
                    {
                        string thingID1 = "";
                        string thingID2 = "";
                        string serviceName1 = "";
                        string serviceName2 = "";

                        // prompt user to enter two services
                        Console.WriteLine("Notice: The order entering services 1/2 would effect the realtionship");
                        bool isFail = false; // check if the input user entered is valid or not
                        for (int i = 1; i <= 2; i++)
                        {
                            Console.WriteLine($"Enter ThingID and ServiceName pair for Service {i}: (ThingID ServiceName)");
                            string str = Console.ReadLine();
                            string[] words = str.Split(' ');
                            if (words.Length != 2)
                            {
                                Console.WriteLine("Please enter thingID and serviceName seperated by a space");
                                isFail = true;
                                break;
                            }
                            if (!pp.SVH.isServiceExist(words[0], words[1]))
                            {
                                isFail = true;
                                break;
                            }

                            if (i == 1)
                            {
                                thingID1 = words[0];
                                serviceName1 = words[1];
                            }
                            else
                            {
                                thingID2 = words[0];
                                serviceName2 = words[1];
                            }

                        }
                        if (isFail) continue;

                        /* Prompt user for relationship selection */
                        List<string> relations = pp.SVH.showMatchRelation(thingID1, thingID2, serviceName1, serviceName2);
                        // list all the possible relationships here
                        // list the other default relatoinships that can be used (don't show interfere if match the case)
                        Console.WriteLine("Please enter a prefered relationship:");
                        string inputRelation = Console.ReadLine();
                        if (!relations.Contains(inputRelation))
                        {
                            Console.WriteLine("Wrong input for relationship...");
                            continue;
                        }
                        if (Array.FindAll(pp.SVH.defaultRelations, s => s.Equals(inputRelation)) == null)
                        {
                            inputRelation = pp.SVH.thingRelationships[thingID1][inputRelation].type;
                        }

                        Console.WriteLine("The relationship type is: {0}", inputRelation);

                        /* Finalize the APP for future use */
                        APP_Handler app = new APP_Handler();
                        app.numService = 2;
                        app.relation = inputRelation;
                        app.SPI1 = serviceName1;
                        app.SPI2 = serviceName2;
                        app.tweetSC1 = pp.SVH.genServiceCallTweet(thingID1, serviceName1, pp.SVH.getServiceInput(thingID1, serviceName1));
                        app.expectResult1 = pp.SVH.getExpectedResult(thingID1, serviceName1);
                        app.tweetSC2 = pp.SVH.genServiceCallTweet(thingID2, serviceName2, pp.SVH.getServiceInput(thingID2, serviceName2));
                        app.expectResult2 = pp.SVH.getExpectedResult(thingID2, serviceName2);
                        app.hasOutputSC1 = pp.SVH.hasOuput(thingID1, serviceName1);
                        app.hasOutputSC2 = pp.SVH.hasOuput(thingID2, serviceName2); ;
                        app.ipAddrSC1 = pp.IDP.thingLanguageTweets[thingID1].thingIP;
                        app.ipAddrSC2 = pp.IDP.thingLanguageTweets[thingID2].thingIP; ;
                        app.port1 = app.port2 = 6668;

                        /* maybe show the user what is the app setting right now*/
                        Console.WriteLine("Enter a name for this APP or \"clear\" to give up:");
                        string userInput = Console.ReadLine();
                        if (userInput == "clear")
                        {
                            Console.WriteLine("Clear the recipe settings... Done");
                            continue;
                        }
                        app.appName = userInput;
                        pp._APP.Add(app);
                        Console.WriteLine("Your APP {0} has been finalized, please activate it thru app command", app.appName);
                    }
                    else
                    {
                        Console.WriteLine("Wrong Input");
                        continue;
                    }

                }
                else if (inputStr == "app")
                {
                    Console.WriteLine("All the APPs we have right now:");
                    if (pp._APP.Count == 0)
                    {
                        Console.WriteLine("There is no any APP created...");
                        continue;
                    }
                    int i = 1;
                    foreach (var entry in pp._APP)
                    {
                        Console.Write("{0}. ", i);
                        Console.WriteLine(entry.appName);
                        i++;
                    }
                    Console.WriteLine("Enter the APP you woulid like to activate:");
                    APP_Handler curAPP = pp.getAPP(pp._APP, Console.ReadLine());
                    if (curAPP == null)
                    {
                        Console.WriteLine("The APP you entered is not exist");
                        continue;
                    }

                    int ret = pp.activateAPP(curAPP);
                    if (ret < 0)
                    {
                        Console.WriteLine("Failed to activate the APP");
                        continue;
                    }
                    Console.WriteLine("\nDone acivation of APP: {0}\n", curAPP.appName);


                }
                else if (inputStr == "exit")
                {
                    break;
                }
                else
                {
                    continue;
                }
                    
            }
        }
    }
}
