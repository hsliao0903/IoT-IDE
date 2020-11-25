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
