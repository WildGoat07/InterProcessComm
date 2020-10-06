﻿using Interprocomm;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    #region Private Fields

    private static Mutex mutex;

    #endregion Private Fields

    #region Public Methods

    public static async Task Main()
    {
        bool created;
        mutex = new Mutex(true, "interprocomm", out created);
        if (created)
        {
            //we create the server object
            var server = new Server("interprocomm key");
            //if the client is disconnected, we simply close the server here
            server.ClientDisconnected += () => server.Close();
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
        }
        else
        {
            //we create the client object
            var client = new Client("interprocomm key");
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
        }
    }

    #endregion Public Methods
}