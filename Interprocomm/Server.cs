using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interprocomm
{
    /// <summary>
    /// Class used as the server of the application. It will recieve requests from the client
    /// application and send back a response if needed.
    /// </summary>
    public class Server : IDisposable
    {
        #region Private Fields

        private bool closed;
        private bool[] connected;
        private Task[] runningServers;
        private int runningServersCount;
        private NamedPipeServerStream[] serverStreams;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">
        /// Key used to sync the server and the client. It must match the one used for the client.
        /// </param>
        /// <param name="serverCount">
        /// Number of servers up for a connection (number of clients connected at the same time).
        /// When all servers are busy, the next client has to wait before connecting.
        /// </param>
        public Server(string key, byte serverCount = 4)
        {
            ServerCount = serverCount;
            closed = true;
            connected = new bool[serverCount];
            serverStreams = new NamedPipeServerStream[serverCount];
            for (int i = 0; i < serverCount; ++i)
                connected[i] = false;
            Key = key;
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Event triggered when a client is connected.
        /// </summary>
        public event Action ClientConnected;

        /// <summary>
        /// Event triggered when a client is disconnected.
        /// </summary>
        public event Action ClientDisconnected;

        /// <summary>
        /// Event triggered when the client sent a request.
        /// </summary>
        public event Action<Request> RequestRecieved;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Returns true if the server is connected with at least one client.
        /// </summary>
        public bool Connected => connected.Contains(true);

        /// <summary>
        /// Returns the number of connected clients to the server.
        /// </summary>
        public int ConnectedClientCount => connected.Count(c => c);

        /// <summary>
        /// The key used for the server and the client
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Returns true if the server is started and connected/ready to be connected
        /// </summary>
        public bool Open => !closed;

        /// <summary>
        /// Returns the number of available servers, or the max number of client connected at the
        /// same time
        /// </summary>
        public byte ServerCount { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Closes the server. The Start() method will end and needs to be recalled to reuse the
        /// server. Same as Dispose()
        /// </summary>
        public void Close() => Dispose();

        /// <summary>
        /// Dispose the server. Same as Close()
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < ServerCount; ++i)
                connected[i] = false;
            closed = true;
            foreach (var item in serverStreams)
                item.Dispose();
        }

        /// <summary>
        /// Starts the server. Can only be started once. Method stopped when calling Close() or Dispose().
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (!Open)
            {
                closed = false;
                runningServers = new Task[ServerCount];
                runningServersCount = 0;
                for (int i = 0; i < ServerCount; i++)
                    runningServers[i] = Task.Run(runServer);
                await Task.WhenAll(runningServers);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void runServer()
        {
            int id = runningServersCount++;
            var server = new NamedPipeServerStream(Key, PipeDirection.InOut, ServerCount);
            serverStreams[id] = server;
            while (!closed)
            {
                try
                {
                    var task = server.WaitForConnectionAsync();
                    while (!task.IsCompleted)
                    {
                        Thread.Sleep(50);
                        if (closed)
                            return;
                    }
                    connected[id] = true;
                    ClientConnected?.Invoke();
                    while (!closed)
                    {
                        var bit = (byte)server.ReadByte();
                        var bitSize = new byte[4];
                        bitSize[0] = bit;
                        server.Read(bitSize, 1, 3);
                        var size = BitConverter.ToInt32(bitSize, 0);
                        var data = new byte[size];
                        server.Read(data, 0, size);
                        if (size != 255 || data.Any(b => b != 0)) //disconnected clients send a 255 byte long request of "0" for some reason
                        {
                            var req = new Request(data);
                            RequestRecieved?.Invoke(req);
                            if (req.response != null)
                            {
                                var respBitSize = BitConverter.GetBytes(req.response.Length);
                                server.Write(new byte[] { 0 }, 0, 1);
                                server.Write(respBitSize, 0, 4);
                                server.Write(req.response, 0, req.response.Length);
                            }
                            else
                                server.Write(new byte[] { 1 }, 0, 1);
                            server.Flush();
                        }
                        else
                            throw new IOException();
                    }
                }
                catch (IOException)
                {
                    server.Disconnect();
                    server.Dispose();
                    connected[id] = false;
                    ClientDisconnected?.Invoke();
                    server = new NamedPipeServerStream(Key, PipeDirection.InOut, ServerCount);
                    serverStreams[id] = server;
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        #endregion Private Methods
    }
}