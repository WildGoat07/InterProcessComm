using Interprocomm;
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
            var server = new Server("interprocomm");
            server.ClientDisconnected += () => server.Dispose();
            server.ClientConnected += () => Console.WriteLine("found client");
            server.RequestRecieved += r =>
            {
                Console.WriteLine(r.StringData);
                r.Respond("res : " + r.StringData);
            };
            await server.Start();
        }
        else
        {
            var client = new Client("interprocomm");
            client.Start();
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                    break;
                Console.WriteLine(client.SendRequest(input)?.StringData);
            }
        }
    }

    #endregion Public Methods
}