using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ServiceParser;
using CNT5517_Project;

namespace ServiceHandler
{
    class Service_Handler
    {
        // suppose the Thing ID is unique, and all the service name is also unique
        public Dictionary<string, Dictionary<string, Tservice>> thingServiceTweets;
		// suppose the Thing ID is unique, and all the relationship name in also unique
		public Dictionary<string, Dictionary<string, TRelation>> thingRelationships;
		private Service_Helper_Functions SVHelper = new Service_Helper_Functions();
		public string[] defaultRelations = {"control",
											"drive",
											"support",
											"extent",
											"contest",
											"interfere"};
		public string[] defaultRelationsDesc = {"control\t\t\t(If A THEN B)",
											"drive\t\t\t(USE A TODO B)",
											"support\t\t\t(BEFORE A CHECK ON B)",
											"extent\t\t\t(DO A WHILE DOING B)",
											"contest\t\t\t(Not supported by IDE yet)",
											"interfere\t\t(Not supported by IDE yet)"};
		public string[] defaultRelationsDescV2 = {"If A THEN B",
											"USE A TODO B",
											"BEFORE A CHECK ON B",
											"DO A WHILE DOING B",
											"Not supported by IDE yet",
											"Not supported by IDE yet"};
		public Service_Handler()
		{
			thingServiceTweets = new Dictionary<string, Dictionary<string, Tservice>>();
			thingRelationships = new Dictionary<string, Dictionary<string, TRelation>>();
			thingServiceTweets.Clear();
			thingRelationships.Clear();
		}


		public bool isServiceExist(string thingID, string serviceName)
		{
			
			/* Check if the service name or thing ID exist */
			if (!thingServiceTweets.ContainsKey(thingID) || !thingServiceTweets[thingID].ContainsKey(serviceName))
			{
				Console.WriteLine("{1} or {0} doesn't exist yet\n", serviceName, thingID);
				return false;
			}
			return true;
		}

		/**********************************************************************/
		/*  For Service Tweets                                                */
		/**********************************************************************/

		public void parse_ServiceTweets(string tweet)
		{
			JObject jsonOBJ = JObject.Parse(tweet);

			Tservice tInfo = new Tservice();
			tInfo.tweetType =				(string)jsonOBJ["Tweet Type"];
			tInfo.serviceName =				(string)jsonOBJ["Name"];
			tInfo.thingID =					(string)jsonOBJ["Thing ID"];
			tInfo.entityID =				(string)jsonOBJ["Entity ID"];
			tInfo.smartspaceID =			(string)jsonOBJ["Space ID"];
			tInfo.type =					(string)jsonOBJ["Type"];
			tInfo.appCategory =				(string)jsonOBJ["AppCategory"];
			tInfo.description=				(string)jsonOBJ["Description"];
			tInfo.API         =				(string)jsonOBJ["API"];
			tInfo.vendor =					(string)jsonOBJ["Vendor"];
			tInfo.keywords   =				(string)jsonOBJ["Keywords"];
			tInfo.APIstruct =				SVHelper.parse_APIstructure(tInfo.API);

			Dictionary<string, Tservice> serviceDic = new Dictionary<string, Tservice>();
			serviceDic.Add(tInfo.serviceName, tInfo);

			/* Store it if ew can't find its Thing ID */
			if (!thingServiceTweets.ContainsKey(tInfo.thingID))
			{
				/* Can't find its Thing ID, means it is a new service tweet */
				thingServiceTweets.Add(tInfo.thingID, serviceDic);
			}
			else
			{
				/* Check if it is a new service for this Thing ID */
				if (!thingServiceTweets[tInfo.thingID].ContainsKey(tInfo.serviceName))
				{
					/* Add new service to this Thing ID*/
					thingServiceTweets[tInfo.thingID].Add(tInfo.serviceName	, tInfo);

				}
			}
		}

		public void display_ServiceTweets()
		{
			/* display service tweets brief information*/
			Console.WriteLine("{0,-20}{1,-20}{2,-20}{3,-25}{4,-25}{5,-25}", "[SpaceID]","[ThingID]","[Name]","[Input 1]","[Input 2]","[Output]");
			foreach (KeyValuePair<string, Dictionary<string, Tservice>> entry in thingServiceTweets)
			{
				foreach (KeyValuePair<string, Tservice> entry2 in entry.Value)
				{
					
					Console.Write("{0,-20}", entry2.Value.smartspaceID);
					Console.Write("{0,-20}", entry.Key); // thing ID
					Console.Write("{0,-20}", entry2.Value.serviceName);
					Console.Write("{0,-25}", entry2.Value.APIstruct.inputDescription);
					Console.Write("{0,-25}", entry2.Value.APIstruct.inputDescription2);
					Console.Write("{0,-25}", entry2.Value.APIstruct.outputDescription);
					Console.WriteLine();
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}





		public void showServiceAPI(string thingID, string serviceName, string option)
		{
			//Console.WriteLine("Try to show serviceAPI for ThingID:{1}  ServiceName:{0}", serviceName, thingID);
			/* Check if the service name or thing ID exist */
			if (!thingServiceTweets.ContainsKey(thingID) || !thingServiceTweets[thingID].ContainsKey(serviceName))
			{
				Console.WriteLine("{1} or {0} doesn't exist!\n", serviceName, thingID);
				return;
			}


			string serviceAPI = thingServiceTweets[thingID][serviceName].API;
			Tservice.APIformat formatter = thingServiceTweets[thingID][serviceName].APIstruct;
			Console.WriteLine(serviceAPI);
			if (option == "input")
			{
				if (formatter.numInputs == 0)
				{
					Console.WriteLine("No required Input, default iput will be NULL");
				}
				else
				{
					Console.WriteLine("Required number of integer Input(s): {0}", formatter.numInputs);
					Console.WriteLine("\tDescription of Input 1: {0}\n\tDescription of Input 2: {1}\n", formatter.inputDescription, formatter.inputDescription2);
				}
			}
			else if (option == "output")
			{
				if (formatter.numOutputs == 0)
				{
					Console.WriteLine("No output, the expected result would be the success of activating this service");
				}
				else
				{
					Console.WriteLine("One integer output for this service");
					Console.WriteLine("What is your expected result for this service?", formatter.numOutputs);
					Console.WriteLine("\tDescription of output: {0}", formatter.outputDescription);
					
				}
			}
			else
			{
				// show both
				Console.WriteLine("API:  {0}", serviceAPI);
				Console.WriteLine("Input numbers: {0}", formatter.numInputs);
				Console.WriteLine("\t{0} {1}", formatter.inputDescription, formatter.inputDescription2);
				Console.WriteLine("Output numbers: {0}", formatter.numOutputs);
				Console.WriteLine("\t{0}", formatter.outputDescription);
				Console.WriteLine();
			}
		}

		public string genServiceCallTweet(string thingID, string serviceName, string inputStr)
		{

			//Console.WriteLine("\nTry to establish service call tweet for ThingID: {1}  ServiceName:{0} Input arg: {2}", serviceName, thingID, inputStr);
			/* Check if the service name or thing ID exist */
			if (!thingServiceTweets.ContainsKey(thingID) || !thingServiceTweets[thingID].ContainsKey(serviceName))
			{
				//Console.WriteLine("{1} or {0} doesn't exist!\n", serviceName, thingID);
				return null;
			}

			string SSID = thingServiceTweets[thingID][serviceName].smartspaceID;
			string x1 = "\"Tweet Type\" : \"Service call\"";
			string x2 = "\"Thing ID\" : \"" + thingID + "\"";
			string x3 = "\"Space ID\" : \"" + SSID + "\"";
			string x4 = "\"Service Name\" : \"" + serviceName + "\"";
			string x5 = "\"Service Inputs\" : \"" + inputStr + "\"";
			return " { " + x1 + "," + x2 + "," + x3 + "," + x4 + "," + x5 + " }";
		}

		public string getServiceInput(string thingID, string serviceName)
		{
			bool ret = isServiceExist(thingID, serviceName);
			if (!ret) return null;

			showServiceAPI(thingID, serviceName, "input");
			string inputStr = "";

			if (thingServiceTweets[thingID][serviceName].APIstruct.numInputs == 0)
				inputStr = "(NULL)";
			else if (thingServiceTweets[thingID][serviceName].APIstruct.numInputs == 1)
			{
				Console.WriteLine("\nEnter an integer for Input 1: ");

				// check if user entered an integer
				string str = Console.ReadLine();
				if (!int.TryParse(str, out _))
				{
					Console.WriteLine("\nError:Please enter an integer\n");
					return null;
				}
				string[] userInputStr = str.Split(' ');
				inputStr += "(" + userInputStr[0] + ")";
			}
			else
			{
				Console.WriteLine("\nEnter two integers seperate by a space for Input 1 and Input 2");
				Console.Write("ie.\"5 6\" or \"9 -1\": ");

				// check if user entered integers
				string[] userInputStr = Console.ReadLine().Split(' ');
				if (!int.TryParse(userInputStr[0], out _) || !int.TryParse(userInputStr[1], out _))
				{
					Console.WriteLine("\nError: Please enter integers for Input 1 and Input 2\n");
					return null;
				}
				inputStr += "(" + userInputStr[0] + "," + userInputStr[1] + ")";
			}

			//Console.WriteLine("\nThe Input string would be: {0}", inputStr);
			return inputStr;
		}

		public string getExpectedResult(string thingID, string serviceName)
		{
			bool ret = isServiceExist(thingID, serviceName);
			if (!ret) return null;

			showServiceAPI(thingID, serviceName, "output");
			string inputStr = "";

			if (thingServiceTweets[thingID][serviceName].APIstruct.numOutputs == 0)
			{
				//Console.WriteLine("No output result for this service");
				inputStr = null;
			}
			else
			{
				Console.WriteLine("Enter an integer for expected output result: ");
				string str = Console.ReadLine();
				if (!int.TryParse(str, out _))
				{
					Console.WriteLine("\nError: Please enter an integer\n");
					return "";
				}
				string[] userInputStr = str.Split(' ');
				inputStr += userInputStr[0];
			}


			//Console.WriteLine("\nThe output string would be: {0}", inputStr);
			return inputStr;
		}

		public bool hasOuput (string thingID, string serviceName)
		{
			// there will only be one output or no output for a service
			if (!isServiceExist(thingID, serviceName)) return false;
			if (thingServiceTweets[thingID][serviceName].APIstruct.numOutputs == 0) return false;
			else return true;
		}

		/**********************************************************************/
		/*  For Relationship Tweets                                           */
		/**********************************************************************/

		public void parse_RelationTweets(string tweet)
		{
			JObject jsonOBJ = JObject.Parse(tweet);

			TRelation tInfo = new TRelation();
			tInfo.tweetType = (string)jsonOBJ["Tweet Type"];
			tInfo.thingID = (string)jsonOBJ["Thing ID"];
			tInfo.spaceID = (string)jsonOBJ["Space ID"];
			tInfo.name = (string)jsonOBJ["Name"];
			tInfo.owner = (string)jsonOBJ["Owner"];
			tInfo.category = (string)jsonOBJ["Category"];
			tInfo.type = (string)jsonOBJ["Type"];
			tInfo.description = (string)jsonOBJ["Description"];
			tInfo.SPI1 = (string)jsonOBJ["FS name"];  //First Service Name
			tInfo.SPI2 = (string)jsonOBJ["SS name"];  //Second Service Name

			Dictionary<string, TRelation> relationDic = new Dictionary<string, TRelation>();
			relationDic.Add(tInfo.name, tInfo);

			/* Store it if we can't find its Thing ID */
			if (!thingRelationships.ContainsKey(tInfo.thingID))
			{
				/* Can't find its Thing ID, means it is a new service tweet */
				thingRelationships.Add(tInfo.thingID, relationDic);
			}
			else
			{
				/* Check if it is a new service for this Thing ID */
				if (!thingRelationships[tInfo.thingID].ContainsKey(tInfo.name))
				{
					/* Add new service to this Thing ID*/
					thingRelationships[tInfo.thingID].Add(tInfo.name, tInfo);

				}
			}
		}

		public void display_RelationTweets()
		{
			/* display relation tweets brief information*/
			Console.WriteLine("{0,-30}{1,-12}{2,-20}{3,-20}{4,-25}", "[Name]", "[Type]", "[Service 1]", "[Service 2]", "[Description]");
			foreach (KeyValuePair<string, Dictionary<string, TRelation>> entry in thingRelationships)
			{
				foreach (KeyValuePair<string, TRelation> entry2 in entry.Value)
				{

					Console.Write("{0,-30}", entry2.Key);
					Console.Write("{0,-12}", entry2.Value.type); // thing ID
					Console.Write("{0,-20}", entry2.Value.SPI1);
					Console.Write("{0,-20}", entry2.Value.SPI2);
					Console.Write("{0,-25}", entry2.Value.description);
					Console.WriteLine();
				}
				Console.WriteLine();
			}
			Console.WriteLine();

		}
		public void showRelationships(string thingID, string relationName)
		{
			Console.WriteLine("Try to show existing relationshiops for ThingID:{1}  Relationships:{0}", relationName, thingID);

			/* Check if the relation name or thing ID exist */
			if (!thingServiceTweets.ContainsKey(thingID) || !thingServiceTweets[thingID].ContainsKey(relationName))
			{
				Console.WriteLine("{1} or {0} doesn't exist!\n", relationName, thingID);
				return;
			}

			TRelation relationStruct = thingRelationships[thingID][relationName];
			relationStruct.displayInfo();
		}

		// give two services, find if it match any of the received relationshiop tweet
		public List<string> showMatchRelation(string TID1, string TID2, string name1, string name2)
		{
			Console.WriteLine("\nPotential relationships are listed below:\n");
			List<string> potentialRelations = new List<string>();

			Console.WriteLine("{0,-30}{1}", "[Relationships]", "[Description]");
			// search in received relationship tweets first
			if (TID1 == TID2)
			{
				foreach (KeyValuePair<string, Dictionary<string,TRelation>> entry in thingRelationships)
				{
					foreach (KeyValuePair<string, TRelation> entry2 in entry.Value)
					{
						if (entry2.Value.SPI1 == name1 && entry2.Value.SPI2 == name2)
						{
							// this relation if for these two services, so we need it
							string desc = "(" + entry2.Value.type + ") " + entry2.Value.description;
							Console.WriteLine("{0,-30}{1}",entry2.Key, desc);
							potentialRelations.Add(entry2.Key);
						}
					}
				}
			}

			
			foreach (var str in defaultRelations)
			{
				potentialRelations.Add(str);
	
			}
			// list default supported relationships with description
			for (int idx = 0; idx < defaultRelations.Length; idx++)
			{
				Console.WriteLine("{0,-30}{1}", defaultRelations[idx], defaultRelationsDescV2[idx]);
			}
			/*
			foreach (var str in defaultRelationsDescV2)
			{
				Console.WriteLine(str);
			}
			*/
			return potentialRelations;
		}

		/* Extract the information from Service Call Reply Tweet */
		public Reply_Info parse_ServiceReplyTweets(string tweet)
		{
			try
			{
				JObject jsonOBJ = JObject.Parse(tweet);
				Reply_Info tInfo = new Reply_Info();
				tInfo.tweetType = (string)jsonOBJ["Tweet Type"];
				tInfo.thingID = (string)jsonOBJ["Thing ID"];
				tInfo.status = (string)jsonOBJ["Status"];
				tInfo.smartspaceID = (string)jsonOBJ["Space ID"];
				tInfo.statusDesc = (string)jsonOBJ["Status Description"];
				if (tInfo.status == "Successful")
				{
					tInfo.serviceName = (string)jsonOBJ["Service Name"];
					tInfo.serviceResult = SVHelper.getOutputResult((string)jsonOBJ["Service Result"]);
				}
				else
				{
					tInfo.serviceName = null;
					tInfo.serviceResult = null;
				}
				return tInfo;
			}
			catch
			{
				Console.WriteLine("The reply tweet is not in JSON format\n{0}", tweet);
				return null;
			}
		}
	}




	class Service_Helper_Functions
	{
		/* Helper function to split the API string, and get API input/output informations */
		public Tservice.APIformat parse_APIstructure(string APIstring)
		{

			/* API format:		serviceName:[input information]:(output informaiton)*/
			/* input format:	[NULL] or [input,int, NULL] or [input,int, NULL|input2,int, NULL] */
			/* output format:	(NULL) or (output,int, NULL)  */
			/* AtlasMiddleWare support at most 1 int ouput and atmost 2 inputs, all the potential types are int */

			Tservice.APIformat formatter = new Tservice.APIformat();
			if (APIstring == "") return formatter;
			string[] APIstrDelimiter = { ":[", "]:(", ":(", ")" };
			char[] APIwordsDelimiter = { ',', '|' };
			string[] APIstr = APIstring.Split(APIstrDelimiter, StringSplitOptions.RemoveEmptyEntries);
			string[] inputWords = APIstr[1].Replace(" ", "").Split(APIwordsDelimiter);
			string[] outputWords = APIstr[2].Replace(" ", "").Split(APIwordsDelimiter);

			/* Deal with inputs */
			if (inputWords.Length == 1)
			{
				// No input
				formatter.numInputs = 0;
				formatter.inputDescription = "void";
				formatter.inputDescription2 = "void";
			}
			else if (inputWords.Length == 3)
			{
				// One input
				formatter.numInputs = 1;
				formatter.inputDescription = inputWords[0];
				formatter.inputDescription2 = "void";
			}
			else if (inputWords.Length == 6)
			{
				// Two inputs
				formatter.numInputs = 2;
				formatter.inputDescription = inputWords[0];
				formatter.inputDescription2 = inputWords[3];
			}
			else
			{
				// Not handeled input case
				Console.WriteLine("Unexpected {0} input case: {1}\n", APIstr[0], APIstr[1]);

			}

			/* Deal with outputs */
			if (outputWords.Length == 1)
			{
				// No output
				formatter.numOutputs = 0;
				formatter.outputDescription = "void";
			}
			else if (outputWords.Length == 3)
			{
				// One output
				formatter.numOutputs = 1;
				formatter.outputDescription = outputWords[0];
			}
			else
			{
				// Not handeled output case
				Console.WriteLine("Unexpected {0} output case: {1}\n", APIstr[0], APIstr[2]);
			}

			return formatter;
		}


		public string getOutputResult(string outputStr)
		{
			if (outputStr == "No Output")
			{
				return null;
			}
			else
			{
				char[] delimiter = {','};
				string[] words = outputStr.Replace(" ", "").Split(delimiter);
				return words[0];
			}
		}
	}




}