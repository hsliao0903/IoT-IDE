using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ServiceParser
{
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
}