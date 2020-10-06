using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Interprocomm
{
    public class Client : IDisposable
    {
        #region Private Fields

        private NamedPipeClientStream clientStream;

        #endregion Private Fields

        #region Public Constructors

        public Client(string key) => Key = key;

        #endregion Public Constructors

        #region Private Properties

        private string Key { get; }

        #endregion Private Properties

        #region Public Methods

        public void Dispose()
        {
            clientStream.Dispose();
        }

        public Request SendRequest(byte[] data)
        {
            var bitSize = BitConverter.GetBytes(data.Length);
            clientStream.Write(bitSize, 0, 4);
            clientStream.Write(data, 0, data.Length);
            clientStream.Flush();
            byte bit = (byte)clientStream.ReadByte();
            if (bit == 1)
                return null;
            var respSizeBit = new byte[4];
            clientStream.Read(respSizeBit, 0, 4);
            var size = BitConverter.ToInt32(respSizeBit, 0);
            var content = new byte[size];
            clientStream.Read(content, 0, size);
            return new Request(content);
        }

        public Request SendRequest(string stringData) => SendRequest(Encoding.UTF8.GetBytes(stringData));

        public void Start()
        {
            clientStream = new NamedPipeClientStream(".", Key, PipeDirection.InOut);
            clientStream.Connect();
        }

        #endregion Public Methods
    }
}