# IoT-IDE
IoT-IDE project for CNT5517 mobile computing

It is user IDE console version to manipulate IoT applicatoins based on the AtlasFramework showing below:
https://github.com/AtlasFramework/AtlasThingMiddleware_RPI

User could build Smart Space Applications and later activate them by adding "Service" and "Relationship" to it.

Develop Environment and packages:
1. .NET Core 3.1
3. Newtonsoft.JSON package

Intros for the Terms using:

Atlas: A program that helps to send/receive tweets according to IoT-DDL files, and help execute the services.

Thing: In our cases, RaspberryPIs with Atlas running

Entities: There might be more than one entity implementing on a Thing. For example, a button / a light / a temperature sensor

Services: Each entity might provide more than one service. Take “button” as an example, detect a button press is a service, detect a quick double press is also another service.

Relationships: Each service might have 0~N relationships with other services. However, it is not a necessity. The relationships are defined from IoT-DDL file, and sent as tweets by Atlas. Users could use them to develop applications, it is all up to users. 

Tweets: A way to communicate and notify others about the Thing itself. Somehow it is like a standard communication protocol. Every “IoT Thing” that runs Atlas are able to communicate to each other. And for this project, our IDE should manage to collect Tweets, to show the user what kinds of Things / Entities / Services / Relationships we have in the VSS right now. And provide an intelligence way for users to build applications based on those resources.


Tweet Types:  

Identity_Thing: basic informations about the Thing

Identity_Language: network protocol infos for the Thing

Identity_Entity: informations about the entity

Service: information about the services provide by each entity

Service Call: to activate the service

Service call reply: the status after activated the service


Usage Case Example:

Thing: RPi running "AtlasMiddleware" connected to WIFI
  Entity 1: A sound sensor
    Service: Teturn true if any noise is detected in a peroid of time

  Entity 2: A LED
    Service 1: Turn on the LED
    Service 2: Turn off the LED

IoT-DDL File: Create a IoT-DDL xml file, define the IoT Service and Entity for RPi, if this case the Sound Sensor and LED

AtlasMiddleware: It will start to send "Tweets" to a specific multicast group.

IDE program: Capture Tweets from the specific multicast group and stores it
  
IDE Recipe cmd: User could create IoT Applications based on the informations that IDE has captured
  Noise Warning: In a period of time, if any noise is detected by the Sound Sensor, turn on a LED for warning
    Service A: Detect any noise for 10 mins, expect result would be true
    Service B: Turn on the LED
    Relaion: Drive (USE A to do B)
  
IDE APP cmd: Activate a application that is already well difined by user
  Noise Warning: 
    1. IDE sends a sercice call to RPi for Service A in this application, which is noise detection in 10 minutes.
    2. If the any noise is detected, RPi will send a reply tweet back to IDE, specifying a noise is detected.
    3. IDE checks the reply tweet, compare it to expected result defined by user, which is true in this case
    4. Because Service A is successfully activated and it matches the expected result, IDE sends a service call for Service B
    5. RPi turn on the LED according to IDE's service call and then send back a reply
    6. IDE received the reply for turnning on the LED
    7. This applicatoin is successfully acivated and done

Relsionshiop types involving Services:

They help users to define the action and behavior for multiple Services in an IoT application, provide some sort of flexiblity and clearance
Control: IF A THEN B
Drive: USE A TODO B
Support: BEFORE A CHECK ON B
Extent: DO A WHIL DOING B
Contest: PREFER USING A OVER B
Interfere: DO NOT DO A IF DOING B
