using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Interprocomm.Server
{
    public class Server : IDisposable
    {
        #region Private Fields

        private NamedPipeServerStream serverStream;

        #endregion Private Fields

        #region Public Constructors

        public Server(string key)
        {
            Key = key;
        }

        #endregion Public Constructors

        #region Public Events

        public event Action ClientConnected;

        public event Action ClientDisconnected;

        public event Action<Request> RequestRecieved;

        #endregion Public Events

        #region Public Properties

        public String Key { get; }

        #endregion Public Properties

        #region Public Methods

        public void Dispose()
        {
            serverStream.Dispose();
        }

        public async Task Start()
        {
            await Task.Run(() =>
            {
                serverStream = new NamedPipeServerStream(Key, PipeDirection.InOut);
                while (true)
                {
                    try
                    {
                        serverStream.WaitForConnection();
                        ClientConnected?.Invoke();
                        while (true)
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
                                serverStream.Write(respBitSize, 0, 4);
                                serverStream.Write(req.response, 0, req.response.Length);
                                serverStream.Flush();
                            }
                        }
                    }
                    catch (IOException)
                    {
                        serverStream.Disconnect();
                        serverStream.Dispose();
                        ClientDisconnected?.Invoke();
                        serverStream = new NamedPipeServerStream(Key, PipeDirection.InOut);
                    }
                }
            });
        }

        #endregion Public Methods
    }
}