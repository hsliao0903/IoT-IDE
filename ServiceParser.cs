using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ServiceParser
{
    // for service tweet
    class Tservice
    {
        

        public string tweetType;
        public string serviceName;               // given name of the service
        public string thingID;               // the unique ID of the thing
        public string entityID;              // the unique ID of the entity in the thing
        public string smartspaceID;               // the unique ID of the smart space
        public string type;                  // Condition, Report or Action
        public string appCategory;           // Automation, Health, Eating, Office, Time Alarms, ...
        public string description;           // Short description by the vendor
        public string API;                   // the API to access the service
        public APIformat APIstruct;          // API details
        public string vendor;                // the vendor of this service
        public string keywords;              // keywords that describe the service
        

        public struct APIformat
        {
            public int numInputs;
            public int numOutputs;
            public string inputDescription;
            public string inputDescription2;
            public string outputDescription;
        }

        public void displayInfo()
        {
            Console.WriteLine("Display service Info: ");
            Console.WriteLine("name: "  + serviceName );
            Console.WriteLine("thing id: " + thingID);
            Console.WriteLine("entity id: " + entityID);
            Console.WriteLine("space id: " + smartspaceID);
            Console.WriteLine("application category: " + appCategory);
            Console.WriteLine("type: " + type );
            Console.WriteLine("description: " + description);
            Console.WriteLine("API: " + API);
            Console.WriteLine("Keywords: " + keywords);
            Console.WriteLine("--------------------------");
            
        }
    }

    // for relationship tweet
    class TRelation
    {
        public string tweetType;
        public string thingID;       // the unique ID of the thing
        public string spaceID;       // the unique ID of the smart space
        public string name;          // given name of the relationship
        public string owner;         // who established this (vendor/developer or discovered by thing)
        public string category;      // Cooperative or Competitive
        public string type;          // Control/Drive/Support/Extend or Contest/Interfere/Refine/Subsume
        public string description;   // Short description by the vendor
        public string SPI1;      //first endpoint of the relationship, two services for now
        public string SPI2;      //second endpoint of the relationship, two services for now


        

        public void displayInfo()
        {
            Console.WriteLine("Display relationship Info: ");
            Console.WriteLine("name: " + name);
            Console.WriteLine("category: " + category);
            Console.WriteLine("type: " + type);
            Console.WriteLine("description: " + description);
            Console.WriteLine("Service1: " + SPI1);
            Console.WriteLine("Service2: " + SPI2);
            Console.WriteLine("--------------------------");
            
        }
    }

    // structure for APPs
    class APP_Handler
    {
        public string appName;      // name of the app
        public int numService;      // number of services in this APP, at least 1, at most 2 for now
        public string SPI1;         // service 1
        public string SPI2;         // service 2
        public string inputStr1;    // the input string for generating service 1 call tweet
        public string inputStr2;    // the input string for generating service 2 call tweet
        public string tweetSC1;     // service call tweet for service 1
        public string tweetSC2;     // service call tweet for service 2
        public bool hasOutputSC1;   // does service 1 has ouput?  if ture we could show it as int
        public bool hasOutputSC2;   // does service 2 has output?
        public string expectResult1;      
        public string expectResult2; // the expected result
        public string relation;     // if numService = 2, we could have relationships
        public string ipAddrSC1;    // ip address for service 1, they might be supported by diff "Thing"
        public string ipAddrSC2;    // ip address for service 2
        public int port1;
        public int port2;



    }

    // for service call reply tweets
    class Reply_Info
    {
        public string tweetType;
        public string serviceName;               // name of the service call reply
        public string thingID;               // the unique ID of the thing
        public string smartspaceID;               // the unique ID of the smart space
        public string status;                  // status of the service call
        public string statusDesc;           // Status Description
        public string serviceResult;           // result of the service call, null or an integer
        

    }
}