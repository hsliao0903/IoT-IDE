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

                    //Console.WriteLine("Joining multicast address group 232.1.1.1:1235 ");
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
            //IDP.display_IdentityTweets();
            Console.WriteLine("Identity Thing and Language Tweets:");
            IDP.display_Valid_Things();
            Console.WriteLine();
            //IDP.display_LanguageTweets();
            //Console.WriteLine();
            Console.WriteLine("Identity Entity Tweets:");
            IDP.display_EntityTweets();
            Console.WriteLine();
            Console.WriteLine("Service Tweets:");
            SVH.display_ServiceTweets();
            Console.WriteLine();
            Console.WriteLine("Relationship Tweets:");
            SVH.display_RelationTweets();
            Console.WriteLine();


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

        private APP_Handler getAPP(List<APP_Handler> appStruct ,int appIdx)
        {
            // check if the APP exist
            int idx = 0;
            foreach (var entry in appStruct)
            {
                idx++;
                if (idx == appIdx)
                {
                    return entry;
                }

            }
            return null;
        }

        private void showAPPInfoBrief(APP_Handler curAPP, int option)
        {
            if (option == 1)
                Console.WriteLine("\n------------------ACTIVATION--------------------");
            else if (option == 2)
                Console.WriteLine("\n-------------------SUCCEED----------------------"); 
            else
                Console.WriteLine("\n-------------------FAILED-----------------------");
            switch (curAPP.relation)
            {
                case "control":
                    Console.WriteLine("{0}: IF \"{1}\" THEN \"{2}\"",curAPP.appName, curAPP.SPI1, curAPP.SPI2);
                    break;
                case "drive":
                    Console.WriteLine("{0}: USE \"{1}\" TODO \"{2}\"", curAPP.appName, curAPP.SPI1, curAPP.SPI2);
                    break;
                case "support":
                    Console.WriteLine("{0}: BEFORE \"{1}\" CHECK ON \"{2}\"", curAPP.appName, curAPP.SPI1, curAPP.SPI2);
                    break;
                case "extent":
                    Console.WriteLine("{0}: DO \"{1}\" WHILE DOING \"{2}\"", curAPP.appName, curAPP.SPI1, curAPP.SPI2);
                    break;
                default:
                    Console.WriteLine("{0}: \"{1}\"", curAPP.appName, curAPP.SPI1);
                    break;
            }
            Console.WriteLine("------------------------------------------------\n");
        }

        private void showBriefTmpSettingsAPP(APP_Handler app)
        {
            Console.WriteLine("\nBrief APP settings you just made:");

            // show service 1 settings
            Console.WriteLine("{0,-30}{1,-30}{2,-30}", "[Service 1]", "[Input(s)]", "[Expect Result]");
            Console.Write("{0,-30}{1,-30}",app.SPI1, app.inputStr1);
            if (app.expectResult1 == null) Console.WriteLine("{0,-30}", "Successfull Call");
            else Console.WriteLine("{0,-30}", "(" + app.expectResult1 + ")");
            Console.WriteLine();

            if (app.numService != 2) return;

            // show service 2 settings
            Console.WriteLine("{0,-30}{1,-30}{2,-30}", "[Service 2]", "[Input(s)]", "[Expect Result]");
            Console.Write("{0,-30}{1,-30}", app.SPI2, app.inputStr2);
            if (app.expectResult2 == null) Console.WriteLine("{0,-30}", "Successfull Call");
            else Console.WriteLine("{0,-30}", "(" + app.expectResult2 + ")");
            Console.WriteLine();

            // show relationshiop settings
            Console.WriteLine("{0,-30}{1,-30}", "[Relationship Type]", "[Description]");
            Console.Write("{0,-30}", app.relation);
            int idx = Array.IndexOf(SVH.defaultRelations, app.relation);
            if (idx > -1) Console.WriteLine("{0,-30}", SVH.defaultRelationsDescV2[idx]);
            else Console.WriteLine("{0,-30}", app.relation);

        }

        private bool isServiceSucceed(int sID, APP_Handler curAPP, Reply_Info reply)
        {
            if (reply == null) return false;
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
                if (isServiceSucceed(1, curAPP, reply))
                {
                    showReply(reply);
                    Console.WriteLine("\nAPP \"{0}\" successfully activated!", curAPP.appName);
                    return 0;
                }
                else
                {
                    showReply(reply);
                    Console.WriteLine("\nAPP \"{0}\" failed because an unseccessfull service call or the replied result is not expected", curAPP.appName, curAPP.SPI1);
                    return -1;
                }

            }
            else if (curAPP.numService == 2)
            {
                // deal with each relation between these two services
                string replyTweet = null;
                Reply_Info reply = null;
                switch (curAPP.relation)
                {
                    // IF A then B , USE A to do B , they all need to make sure service A is successfully activated first then do B
                    case "control": 
                    case "drive":
                        // execute service A first
                        replyTweet = SendServiceCallTweets(curAPP.tweetSC1, curAPP.ipAddrSC1, curAPP.port1);
                        reply = SVH.parse_ServiceReplyTweets(replyTweet);

                        // check the IF predicate for service A, is the reply expected?
                        if (isServiceSucceed(1, curAPP, reply))
                        {
                            showReply(reply);
                            // pass the IF predicate then do service B
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
                                Console.WriteLine("\nAPP \"{0}\" failed because an unseccessfull service call or the replied result is not expected for \"{1}\"", curAPP.appName, curAPP.SPI2);
                                return -1;
                            }
                        }
                        else
                        {
                            // false do not do service B
                            showReply(reply);
                            Console.WriteLine("\nAPP \"{0}\" failed because an unseccessfull service call or the replied result is not expected for \"{1}\"", curAPP.appName, curAPP.SPI1);
                        }
                        break;

                    // Check B then do A , while doing B Do A , they all need to make sure service B is successfully activated first then do A
                    // just like the Control and Drive relation, but swap the order of service A and B
                    case "support":
                    case "extend":
                        // activate service B first
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
                                Console.WriteLine("\nAPP \"{0}\" failed because an unseccessfull service call or the replied result is not expected for \"{1}\"", curAPP.appName, curAPP.SPI1);
                                return -1;
                            }
                        }
                        else
                        {
                            // if false do not do service A
                            showReply(reply);
                            Console.WriteLine("\nAPP \"{0}\" failed because an unseccessfull service call or the replied result is not expected for \"{1}\"", curAPP.appName, curAPP.SPI2);
                        }
                        break;
                    case "contest":
                        Console.WriteLine("\nAPP \"{0}\" failed because relation \"Contest\" is not supported by IDE yet", curAPP.appName);
                        break;
                    case "interfere":
                        Console.WriteLine("\nAPP \"{0}\" failed because relation \"Interfere\" is not supported by IDE yet", curAPP.appName);
                        break;
                }

            }
            else
            {
                Console.WriteLine("\nError: Cannot activate the APP\n");
                return 0;
            }
            return -1;
        }

        private void showReply(Reply_Info reply)
        {

            Console.WriteLine("\nReply Tweet info for activated service \"{0}\"", reply.serviceName);
            Console.WriteLine("{0,-20}{1}", "Status:", reply.status);
            Console.WriteLine("{0,-20}{1}", "Description:", reply.statusDesc);
            if (reply.serviceResult == null)
                Console.WriteLine("{0,-20}{1}\n", "Result:", "N/A");
            else
                Console.WriteLine("{0,-20}{1}\n", "Result:",reply.serviceResult);

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
                    "relation\t(Show \"Relationship\" information briefly)\n" +
                    "showall\t\t(Show all received Tweets in a brief format, for debugging)\n" +
                    "recipe\t\t(Build an APP by selecting 2 services and add a relationship for them)\n" +
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
                    Console.WriteLine("\nExecute cmd thing:\n");
                    pp.IDP.display_Valid_Things();
                }
                else if (inputStr == "entity")
                {
                    Console.WriteLine("\nExecute cmd entity:\n");
                    pp.IDP.display_EntityTweets();
                }
                else if (inputStr == "service")
                {
                    Console.WriteLine("\nExecute cmd service:\n");
                    pp.SVH.display_ServiceTweets();
                }
                else if (inputStr == "relation")
                {
                    Console.WriteLine("\nExecute cmd relation:\n");
                    pp.SVH.display_RelationTweets();
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

                        Console.Write("Enter th ip address: ");
                        string ipaddress = Console.ReadLine();
                        pp.SendServiceCallTweets(tweetServiceCall, ipaddress, 6668);
                    }

                }
                else if (inputStr == "showall")
                {
                    Console.WriteLine("\nExecute cmd showall\n");
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
                    Console.WriteLine("\nExecute cmd recipe\n");
                    Console.WriteLine("To finalize an APP, you could add one or two service(s) for an APP");
                    Console.WriteLine("(Notice: There is only one relation you could add for two services, but none for one service)");
                    Console.WriteLine("How many services you would like to add?");
                    Console.WriteLine("Please enter 1 or 2: ");
                    string nums = Console.ReadLine();
                    

                    /* user choose 1 */
                    if (nums == "1")
                    {
                        Console.WriteLine("Now, please choose the service you would like to add from below");
                        pp.SVH.display_ServiceTweets();
                        Console.WriteLine("Enter the \"ThingID\" and \"ServiceName\" pair seperated by a space below (ThingID ServiceName): ");
                        string str = Console.ReadLine();
                        string[] words = str.Split(' ');
                        if (words.Length != 2)
                        {
                            Console.WriteLine("\nError: Wrong Input format, should be \"ThingID space ServiceName\"\n");
                            continue;
                        }
                        if (!pp.SVH.isServiceExist(words[0], words[1]))
                        {
                            Console.WriteLine("\nError: Cannot find ThingID or ServiceName you entered in IDE\n");
                            continue;
                        } 

                        string thingID = words[0];
                        string serviceName = words[1];

                        /* Set the APP structure */
                        APP_Handler app = new APP_Handler();
                        app.numService = 1;
                        app.relation = null;
                        app.SPI1 = serviceName;
                        app.SPI2 = null;

                        Console.WriteLine("\nPlease set the input and expected output result for this Service if required ...\n");
                        // get a valid input for service 1 from user
                        app.inputStr1 = pp.SVH.getServiceInput(thingID, serviceName);
                        if (app.inputStr1 == null) continue;
                        app.tweetSC1 = pp.SVH.genServiceCallTweet(thingID, serviceName, app.inputStr1);

                        // get a valid expected result for service 1 from user
                        app.expectResult1 = pp.SVH.getExpectedResult(thingID, serviceName);
                        if (app.expectResult1 == "") continue;

                        app.inputStr2 = null;
                        app.tweetSC2 = null;
                        app.expectResult2 = null;
                        app.hasOutputSC1 = pp.SVH.hasOuput(thingID, serviceName);
                        app.hasOutputSC2 = false;
                        app.ipAddrSC1 = pp.IDP.thingLanguageTweets[thingID].thingIP;
                        app.ipAddrSC2 = null;
                        app.port1 = app.port2 = 6668;

                        /* show the user what is the APP setting for now */
                        pp.showBriefTmpSettingsAPP(app);

                        Console.WriteLine("\nEnter an unique name for this APP or \"clear\" to give up: ");
                        string userInput = Console.ReadLine();
                        if (userInput == "clear")
                        {
                            Console.WriteLine("\nClear the current APP settings... Done");
                            continue;
                        }
                        app.appName = userInput;
                        pp._APP.Add(app);
                        Console.WriteLine("\nYour APP \"{0}\" has been finalized, please activate it thru \"app\" command\n", app.appName);

                    }
                    else if (nums == "2")
                    {
                        string thingID1 = "";
                        string thingID2 = "";
                        string serviceName1 = "";
                        string serviceName2 = "";

                        // prompt user to enter two services
                        Console.WriteLine("(Notice: The order of services 1 and 2 would effect the relationship behavior added to them)");
                        bool isFail = false; // if any error happens in selection of two services, we terminate this command
                        for (int i = 1; i <= 2; i++)
                        {
                            if (i == 1)
                                Console.WriteLine($"\nFirst, please choose a service as Service {i} from below: ");
                            else
                                Console.WriteLine($"\nSecond, please choose a service as Service {i} from below: ");

                            // show the service options
                            pp.SVH.display_ServiceTweets();

                            Console.WriteLine("Enter the \"ThingID\" and \"ServiceName\" pair seperated by a space below (ThingID ServiceName): ");
                            string str = Console.ReadLine();
                            string[] words = str.Split(' ');
                            if (words.Length != 2)
                            {
                                Console.WriteLine("\nError: Wrong Input format, should be \"ThingID space ServiceName\"\n");
                                isFail = true;
                                break;
                            }
                            if (!pp.SVH.isServiceExist(words[0], words[1]))
                            {
                                Console.WriteLine("\nError: Cannot find ThingID or ServiceName you entered in IDE\n");
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
                        
                        Console.WriteLine("\nPlease choose a prefered relationship for these two Services: ");
                        string inputRelation = Console.ReadLine();

                        if (!relations.Contains(inputRelation))
                        {
                            Console.WriteLine("\nThe relationship you entered does not exist ...\n");
                            continue;
                        }

                        // if the user select a relationship from tweet which is not the default one
                        // convert it to the type it belongs to, contorl/drive/support...etc

                        int pos = Array.IndexOf(pp.SVH.defaultRelations, inputRelation);
                        if (pos <= -1)
                        {
                            Console.Write("\"{0}\" belongs to ", inputRelation);
                            inputRelation = pp.SVH.thingRelationships[thingID1][inputRelation].type;
                            Console.WriteLine("\"{0}\" relationship type", inputRelation);
                        }
                        if (Array.FindAll(pp.SVH.defaultRelations, s => s.Equals(inputRelation)) == null)
                        {
                            Console.Write("\"{0}\" belongs to ", inputRelation);
                            inputRelation = pp.SVH.thingRelationships[thingID1][inputRelation].type;
                            Console.WriteLine("{0} type", inputRelation);
                        }


                        
                        /*  Set the APP structure */
                        
                        APP_Handler app = new APP_Handler();
                        app.numService = 2;
                        app.relation = inputRelation;
                        app.SPI1 = serviceName1;
                        app.SPI2 = serviceName2;

                        Console.WriteLine("\nPlease set the input and expected output result for Service 1 if required ...\n");
                        // get an valid input for service 1 from user
                        app.inputStr1 = pp.SVH.getServiceInput(thingID1, serviceName1); 
                        if (app.inputStr1 == null) continue;
                        app.tweetSC1 = pp.SVH.genServiceCallTweet(thingID1, serviceName1, app.inputStr1);

                        // get a valid expected result for service 1 from user
                        app.expectResult1 = pp.SVH.getExpectedResult(thingID1, serviceName1);
                        if (app.expectResult1 == "") continue;

                        Console.WriteLine("\nPlease set the input and expected output result for Service 2 if required ...\n");
                        // get an valid input for service 2 from user
                        app.inputStr2 = pp.SVH.getServiceInput(thingID2, serviceName2);
                        if (app.inputStr2 == null) continue;

                        // get a valid expected result for service 2 from user
                        app.tweetSC2 = pp.SVH.genServiceCallTweet(thingID2, serviceName2, app.inputStr2);
                        app.expectResult2 = pp.SVH.getExpectedResult(thingID2, serviceName2);
                        if (app.expectResult1 == "") continue;

                        app.hasOutputSC1 = pp.SVH.hasOuput(thingID1, serviceName1);
                        app.hasOutputSC2 = pp.SVH.hasOuput(thingID2, serviceName2); ;
                        app.ipAddrSC1 = pp.IDP.thingLanguageTweets[thingID1].thingIP;
                        app.ipAddrSC2 = pp.IDP.thingLanguageTweets[thingID2].thingIP; ;
                        app.port1 = app.port2 = 6668;

                        /* maybe show the user what is the APP settings right now*/
                        pp.showBriefTmpSettingsAPP(app);


                        Console.WriteLine("\nEnter an unique name for this APP or \"clear\" to give up: ");
                        string userInput = Console.ReadLine();
                        if (userInput == "clear")
                        {
                            Console.WriteLine("\nClear the current Recipe settings... Done");
                            continue;
                        }
                        else if (userInput == "" || userInput == " " || userInput == "\n" || userInput == "\r\n" || userInput == "\r")
                        {
                            Console.WriteLine("\nError: It is not a valid APP name ...\n");
                            continue;
                        }
                        app.appName = userInput;
                        pp._APP.Add(app);
                        Console.WriteLine("\nYour APP \"{0}\" has been finalized, please activate it thru \"app\" command\n", app.appName);
                    }
                    else
                    {
                        Console.WriteLine("\nError: Wrong Input, please enter 1 or 2\n");
                        continue;
                    }

                }
                else if (inputStr == "app")
                {
                    Console.WriteLine("\nExecute cmd app\n");
                    if (pp._APP.Count == 0)
                    {
                        Console.WriteLine("There is no any APP created for now ...");
                        Console.WriteLine("Please use \"recipe\" command to create new APPs");
                        continue;
                    }

                    Console.WriteLine("All the APPs are listed below: ");
                    int appIdx = 0;
                    Console.WriteLine("{0,-8}{1,-25}{2,-25}{3,-25}{4,-25}", "[ID]", "[APP Name]", "[Service 1]", "[Relationship]", "[Service 2]");
                    foreach (var entry in pp._APP)
                    {
                        //Console.Write("{0}.", i);
                        appIdx++;
                        Console.WriteLine("{4,-8}{0,-25}{1,-25}{2,-25}{3,-25}", entry.appName,entry.SPI1, entry.relation, entry.SPI2, appIdx);
                        
                    }
                    
                    Console.WriteLine("Enter the APP \"ID\" you would like to activate:");
                    //tring inputIdx = ;
                    if (!int.TryParse(Console.ReadLine(), out int idx))
                    {
                        Console.WriteLine("\nError: Please enter an integer for APP ID\n");
                        continue;
                    }
                    APP_Handler curAPP = pp.getAPP(pp._APP, idx) ;
                    if (curAPP == null)
                    {
                        Console.WriteLine("\nError: The APP ID you entered does not exist ...\n");
                        continue;
                    }

                    // show user the basic information about this app
                    pp.showAPPInfoBrief(curAPP, 1);

                    int ret = pp.activateAPP(curAPP);
                    if (ret < 0)
                    {
                        //Console.WriteLine("\nError: Failed to activate the APP \"{0}\"\n", curAPP.appName);
                        pp.showAPPInfoBrief(curAPP, 3);
                        continue;
                    }
                    pp.showAPPInfoBrief(curAPP, 2);
                    //Console.WriteLine("\nSuccessfully activated the APP \"{0}\" once!\n", curAPP.appName);


                }
                else if (inputStr == "exit")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\"{0}\" is a unknown command",inputStr);
                    continue;
                }
                    
            }
        }
    }
}
