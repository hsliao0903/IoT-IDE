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
				Console.WriteLine("{1} or {0} doesn't exist!\n", serviceName, thingID);
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

		





		public void showServiceAPI(string thingID, string serviceName)
		{
			Console.WriteLine("Try to show serviceAPI for ThingID:{1}  ServiceName:{0}", serviceName, thingID);
			/* Check if the service name or thing ID exist */
			if (!thingServiceTweets.ContainsKey(thingID) || !thingServiceTweets[thingID].ContainsKey(serviceName))
			{
				Console.WriteLine("{1} or {0} doesn't exist!\n", serviceName, thingID);
				return;
			}


			string serviceAPI = thingServiceTweets[thingID][serviceName].API;
			Tservice.APIformat formatter = thingServiceTweets[thingID][serviceName].APIstruct;

			Console.WriteLine("API:  {0}", serviceAPI);
			Console.WriteLine("Input numbers: {0}", formatter.numInputs);
			Console.WriteLine("\t{0} {1}", formatter.inputDescription, formatter.inputDescription2);
			Console.WriteLine("Output numbers: {0}", formatter.numOutputs);
			Console.WriteLine("\t{0}", formatter.outputDescription);
			Console.WriteLine();
		}

		public string genServiceCallTweet(string thingID, string serviceName, string inputStr)
		{

			Console.WriteLine("\nTry to establish service call tweet for ThingID: {1}  ServiceName:{0} Input arg: {2}", serviceName, thingID, inputStr);
			/* Check if the service name or thing ID exist */
			if (!thingServiceTweets.ContainsKey(thingID) || !thingServiceTweets[thingID].ContainsKey(serviceName))
			{
				Console.WriteLine("{1} or {0} doesn't exist!\n", serviceName, thingID);
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
			showServiceAPI(thingID, serviceName);
			Console.Write("Enter Input value:");
			string[] userInputStr = Console.ReadLine().Split(' ');
			string inputStr = "";
			if (thingServiceTweets[thingID][serviceName].APIstruct.numInputs == 0)
				inputStr = "(NULL)";
			else if (thingServiceTweets[thingID][serviceName].APIstruct.numInputs == 1)
			{
				inputStr += "(" + userInputStr[0] + ")";
			}
			else
			{
				inputStr += "(" + userInputStr[0] + "," + userInputStr[1] + ")";
			}

			Console.WriteLine("\nThe input string would be: {0}", inputStr);
			return inputStr;
		}

		public bool hasOuput (string thingID, string serviceName)
		{
			
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

		

	}

	class APP_Handler
	{
		public string appName;		// name of the app
		public int numService;		// number of services in this APP, at least 1, at most 2 for now
		public string SPI1;			// service 1
		public string SPI2;			// service 2
		public string tweetSC1;		// service call tweet for service 1
		public string tweetSC2;		// service call tweet for service 2
		public bool hasOutputSC1;	// does service 1 has ouput?  if ture we could show it as int
		public bool hasOutputSC2;   // does service 2 has output?
		public string relation;     // if numService = 2, we could have relationships
		public string ipAddrSC1;    // ip address for service 1, they might be supported by diff "Thing"
		public string ipAddrSC2;    // ip address for service 2
		public int port1;
		public int port2;


	}

}