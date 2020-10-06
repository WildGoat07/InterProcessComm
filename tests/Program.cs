using Interprocomm;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

public class Program
{
    #region Private Fields

    private static Mutex mutex;

    #endregion Private Fields

    #region Public Methods

    public static void Main()
    {
        bool created;
        mutex = new Mutex(true, "interprocomm", out created);
        if (created)
        {
            var server = new NamedPipeServerStream("interprocomm", PipeDirection.InOut);
            Console.WriteLine("server app");
            while (true)
            {
                try
                {
                    server.WaitForConnection();
                    Console.WriteLine("client connected");
                    int value = server.ReadByte();
                    Console.WriteLine("client said : " + value);
                    server.WriteByte((byte)(value * 2));
                    server.Flush();
                    server.Disconnect();
                }
                catch (IOException)
                {
                    server.Disconnect();
                    server.Dispose();
                    server = new NamedPipeServerStream("interprocomm", PipeDirection.InOut);
                }
            }
        }
        else
        {
            var client = new NamedPipeClientStream(".", "interprocomm", PipeDirection.InOut);
            Console.WriteLine("client app");
            client.Connect();
            Console.WriteLine("server connected");
            client.WriteByte(12);
            client.Flush();
        }
    }

    #endregion Public Methods
}