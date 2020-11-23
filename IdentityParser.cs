using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace IdentityParser
{
    class Identity_Parser
    {
		public Dictionary<string, thingInfo> thingIdentityTweets;
		public Dictionary<string, thingLanguage> thingLanguageTweets;
		//public Dictionary<string, List<thingLanguage>> thingEntityTweets;
		public Dictionary<string, Dictionary<string, thingEntity>> thingEntityTweets;

		public struct thingInfo
		{
			//thing information
			public string tweetType;
			public string thingID;
			public string smartspaceID;
			public string thingName;
			public string thingModel;
			public string thingVendor;
			public string thingOwner;
			public string thingDescription;
			public string thingOperatingSystem;
		};

		public struct thingEntity
		{
			//entity information per thing
			public string tweetType;
			public string thingID;
			public string smartspaceID;
			public string entityName;
			public string entityID;
			public string entityType;
			public string entityOwner;
			public string entityVendor;
			public string entityDescription;
		};

		public struct thingLanguage
		{
			//language
			public string tweetType;
			public string thingID;
			public string smartspaceID;
			public string networkName;
			public string networkCommunicationLanguage;
			public string thingIP;
			public string thingPort;
		};	

		public Identity_Parser()
		{
			thingIdentityTweets = new Dictionary<string, thingInfo>();
			thingLanguageTweets = new Dictionary<string, thingLanguage>();
			thingEntityTweets = new Dictionary<string, Dictionary<string, thingEntity>>();
			thingIdentityTweets.Clear();
			thingLanguageTweets.Clear();
			thingEntityTweets.Clear();
		}

		public void parse_LanguageTweets(string tweet)
		{
			JObject jsonOBJ = JObject.Parse(tweet);

			thingLanguage tInfo = new thingLanguage();
			tInfo.tweetType =						(string)jsonOBJ["Tweet Type"];	
			tInfo.thingID =							(string)jsonOBJ["Thing ID"];
			tInfo.smartspaceID =					(string)jsonOBJ["Space ID"];
			tInfo.networkName =						(string)jsonOBJ["Network Name"];
			tInfo.networkCommunicationLanguage =	(string)jsonOBJ["Communication Language"];
			tInfo.thingIP =							(string)jsonOBJ["IP"];
			tInfo.thingPort =						(string)jsonOBJ["Port"];
			

			if (!thingLanguageTweets.ContainsKey(tInfo.thingID))
				thingLanguageTweets.Add(tInfo.thingID, tInfo);
		}

		public void parse_EntityTweets(string tweet)
		{
			JObject jsonOBJ = JObject.Parse(tweet);
			
			thingEntity tInfo = new thingEntity();
			tInfo.tweetType =						(string)jsonOBJ["Tweet Type"];
			tInfo.thingID =							(string)jsonOBJ["Thing ID"];
			tInfo.smartspaceID =					(string)jsonOBJ["Space ID"];
			tInfo.entityName=						(string)jsonOBJ["Name"];
			tInfo.entityID =						(string)jsonOBJ["ID"];
			tInfo.entityType =						(string)jsonOBJ["Type"];
			tInfo.entityOwner =						(string)jsonOBJ["Owner"];
			tInfo.entityVendor =					(string)jsonOBJ["Vendor"];
			tInfo.entityDescription =				(string)jsonOBJ["Description"];

			Dictionary<string, thingEntity> entityDic = new Dictionary<string, thingEntity>();
			entityDic.Add(tInfo.entityID, tInfo);


			/* Store it if it is the new Tweet */
			if (!thingEntityTweets.ContainsKey(tInfo.thingID))
			{
				/* Can't find Thing ID, means it is a new entity tweet */
				
				thingEntityTweets.Add(tInfo.thingID, entityDic);
			}
			else
			{
				/* Check if it is a new entity for this Thing ID */
				if (!thingEntityTweets[tInfo.thingID].ContainsKey(tInfo.entityID))
				{
					/* Add new entity to this Thing ID*/
					thingEntityTweets[tInfo.thingID].Add(tInfo.entityID, tInfo);
					
				}
			}
	
		}


		public void parse_IdentityTweets(string tweet)
		{
			JObject jsonOBJ = JObject.Parse(tweet);

			thingInfo tInfo = new thingInfo();
			tInfo.tweetType =						(string)jsonOBJ["Tweet Type"];
			tInfo.thingID =							(string)jsonOBJ["Thing ID"];
			tInfo.smartspaceID =					(string)jsonOBJ["Space ID"];
			tInfo.thingName =						(string)jsonOBJ["Name"];
			tInfo.thingModel =						(string)jsonOBJ["Model"];
			tInfo.thingVendor =						(string)jsonOBJ["Vendor"];
			tInfo.thingOwner =						(string)jsonOBJ["Owner"];
			tInfo.thingDescription =				(string)jsonOBJ["Description"];
			tInfo.thingOperatingSystem =			(string)jsonOBJ["OS"];



			/* Store it if it is the new Tweet */
			if (!thingIdentityTweets.ContainsKey(tInfo.thingID))
				thingIdentityTweets.Add(tInfo.thingID, tInfo);

		}

		public void display_Valid_Things()
		{
			
			Console.WriteLine("{0,-20}{1,-20}{2,-20}{3,-20}", "[SpaceID]", "[ThingID]", "[IpAddr]", "[Port]");
			foreach (KeyValuePair<string, thingInfo> entry in thingIdentityTweets)
			{
				if (thingLanguageTweets.ContainsKey(entry.Key))
				{
					Console.Write("{0,-20}", entry.Value.smartspaceID);
					Console.Write("{0,-20}", entry.Key);
					Console.Write("{0,-20}", thingLanguageTweets[entry.Key].thingIP);
					Console.Write("{0,-20}", thingLanguageTweets[entry.Key].thingPort);
				}
				Console.WriteLine();
			}
			Console.WriteLine("\n(Notice: Only the Thing recevied its Identity_Tweet and Language_Tweet are shown)\n");
		}
		

		/* Sample to retrieve Identity_Thing tweet data */
		public void display_IdentityTweets()
		{
			Console.WriteLine("[SpaceID]\t[ThingID]\n");
			foreach (KeyValuePair<string, thingInfo> entry in thingIdentityTweets)
			{
					Console.Write("{0,-15}", entry.Value.smartspaceID);
					Console.WriteLine("{0,-15}", entry.Key);
			}
		}

		/*  Sample to retrieve Identity_Language tweet data */
		public void display_LanguageTweets()
		{
			foreach (KeyValuePair<string, thingLanguage> entry in thingLanguageTweets)
			{
				Console.Write("Thing ID:" + entry.Key);
				Console.WriteLine(" Tweet Type:" + entry.Value.tweetType);
			}
		}

		/* Sample to retrieve Identity_Entity tweet data */
		/* It's a dictionary in dictionary structure */
		/* Use Thing ID as outer layer's Key */
		/* Use Entity ID as inner layer's Key  */
		public void display_EntityTweets()
		{
			Console.WriteLine("{0,-20}{1,-20}{2,-20}{3,-20}{4,-20}", "[SpaceID]", "[ThingID]", "[EntityID]", "[Name]","[Description]");
			foreach (KeyValuePair<string, Dictionary<string, thingEntity>> entry in thingEntityTweets)
			{
				
				foreach (KeyValuePair<string, thingEntity> entry2 in entry.Value)
				{
					
					Console.Write("{0,-20}", entry2.Value.smartspaceID);
					Console.Write("{0,-20}", entry.Key); // thing ID
					Console.Write("{0,-20}", entry2.Key); // entity ID
					Console.Write("{0,-20}", entry2.Value.entityName);
					Console.Write("{0,-20}", entry2.Value.entityDescription);
					Console.WriteLine();

				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

	}
}
