# InterProcessComm

> or inter-process communication

InterProcessComm is a very small and simple wrapper for the [NamedPipeServerStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstream) and the [NamedPipeClientStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeclientstream). It allows easy and fast communication between two applications.

### Installation

Use the nuget package manager to install [the lastest version](https://www.nuget.org/packages/interprocesscomm/).

### Getting started

Server side :

```csharp
//we create the server object
//the key must be the same between server/client to enable the connection
var server = new Server("InterProcessComm key");
//if the client is disconnected, we simply close the server here
server.ClientDisconnected += server.Close;
//we can do stuff when a client is found
server.ClientConnected += () => Console.WriteLine("found client");
//we give a purpose to our server when he recieves a request
server.RequestRecieved += r =>
{
    //for now we just write it to the console
    Console.WriteLine(r.StringData);
    //and return "res : " followed by the request
    r.Respond("res : " + r.StringData);
};
//we start the server and wait until it is closed
await server.Start();
```

Client side :

```csharp
//we create the client object
var client = new Client("InterProcessComm key");
//we start the client, it will wait for a connection if
//no server are available with the given key
client.Start();
while (true)//don't do this, it's for presentation purpose only
{
    var input = Console.ReadLine();
    if (input == "exit")
        break;
    //we send the user input to the server and write down the response in the console
    Console.WriteLine(client.SendRequest(input)?.StringData);
}
client.Disconnect();
```