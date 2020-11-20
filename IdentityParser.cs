using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace IdentityParser
{
    class Identity_Parser
    {
		Dictionary<string, thingInfo> thingIdentityTweets;


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
				//public string thingType;
				//public string thingReleaseDate;
		};

		public Identity_Parser()
		{
			thingIdentityTweets = new Dictionary<string, thingInfo>();
			thingIdentityTweets.Clear();
		}

		public void parse_IdentityTweets(string tweet)
		{
			JObject jsonOBJ = JObject.Parse(tweet);
			thingInfo tInfo = new thingInfo();

			tInfo.tweetType = (string)jsonOBJ["Tweet Type"];
			tInfo.thingID = (string)jsonOBJ["Thing ID"];
			tInfo.thingName = (string)jsonOBJ["Name"];
			//tInfo.thingName = tweet;
			Console.WriteLine("\n\nalex\n\n" + tInfo.thingID);
			//tInfo.thingName = jsonArr["Tweet Type"];
			if (!thingIdentityTweets.ContainsKey(tInfo.thingID))
				thingIdentityTweets.Add(tInfo.thingID, tInfo);
		}

		public void display_IdentityTweets(string thingID)
		{
			if (thingIdentityTweets.ContainsKey(thingID))
				Console.WriteLine(thingIdentityTweets[thingID].thingName);
			else
				foreach (KeyValuePair<string, thingInfo> entry in thingIdentityTweets)
				{
					Console.WriteLine(entry.Key + ":" + entry.Value);
				}

		}

	}
}
