using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Interprocomm
{
    public class Server : IDisposable
    {
        #region Private Fields

        private bool closed;
        private bool connected;
        private NamedPipeServerStream serverStream;

        #endregion Private Fields

        #region Public Constructors

        public Server(string key) => Key = key;

        #endregion Public Constructors

        #region Public Events

        public event Action ClientConnected;

        public event Action ClientDisconnected;

        public event Action<Request> RequestRecieved;

        #endregion Public Events

        #region Public Properties

        public bool Connected => !closed;
        public String Key { get; }

        public bool Open => !closed;

        #endregion Public Properties

        #region Public Methods

        public void Close() => Dispose();

        public void Dispose()
        {
            connected = false;
            closed = true;
            serverStream.Dispose();
        }

        public async Task Start()
        {
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