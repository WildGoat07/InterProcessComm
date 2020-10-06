using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
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
        private bool connected;
        private NamedPipeServerStream serverStream;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">
        /// Key used to sync the server and the client. It must match the one used for the client.
        /// </param>
        public Server(string key)
        {
            closed = true;
            connected = false;
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
        /// Returns true if the server is connected with a client.
        /// </summary>
        public bool Connected => !connected;

        /// <summary>
        /// The key used for the server and the client
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Returns true if the server is started and connected/ready to be connected
        /// </summary>
        public bool Open => !closed;

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
            connected = false;
            closed = true;
            serverStream.Dispose();
        }

        /// <summary>
        /// Starts the server. Can only be started once. Method stopped when calling Close() or Dispose().
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (!Open)
                await Task.Run(() =>
                {
                    closed = false;
                    connected = false;
                    serverStream = new NamedPipeServerStream(Key, PipeDirection.InOut);
                    while (!closed)
                    {
                        try
                        {
                            serverStream.WaitForConnection();
                            connected = true;
                            ClientConnected?.Invoke();
                            while (!closed)
                            {
                                var bit = (byte)serverStream.ReadByte();
                                var bitSize = new byte[4];
                                bitSize[0] = bit;
                                serverStream.Read(bitSize, 1, 3);
                                var size = BitConverter.ToInt32(bitSize, 0);
                                var data = new byte[size];
                                serverStream.Read(data, 0, size);
                                var req = new Request(data);
                                RequestRecieved?.Invoke(req);
                                if (req.response != null)
                                {
                                    var respBitSize = BitConverter.GetBytes(req.response.Length);
                                    serverStream.Write(new byte[] { 0 }, 0, 1);
                                    serverStream.Write(respBitSize, 0, 4);
                                    serverStream.Write(req.response, 0, req.response.Length);
                                }
                                else
                                    serverStream.Write(new byte[] { 1 }, 0, 1);
                                serverStream.Flush();
                            }
                        }
                        catch (IOException)
                        {
                            serverStream.Disconnect();
                            serverStream.Dispose();
                            connected = false;
                            ClientDisconnected?.Invoke();
                            serverStream = new NamedPipeServerStream(Key, PipeDirection.InOut);
                        }
                        catch (ObjectDisposedException)
                        {
                            ClientDisconnected?.Invoke();
                        }
                    }
                });
        }

        #endregion Public Methods
    }
}