using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Interprocomm
{
    /// <summary>
    /// Class used as the client of the application. It will send requests to the server and get
    /// back responses from it if needed.
    /// </summary>
    public class Client : IDisposable
    {
        #region Private Fields

        private NamedPipeClientStream clientStream;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">
        /// Key used to sync the server and the client. It must match the one used for the server.
        /// </param>
        public Client(string key) => Key = key;

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Returns true if currently connected to a server.
        /// </summary>
        public bool Connected { get; private set; }

        #endregion Public Properties

        #region Private Properties

        /// <summary>
        /// Returns the key used to connect to the server.
        /// </summary>
        private string Key { get; }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Disconnects the client from the server. Same as Dispose()
        /// </summary>
        public void Disconnect() => Dispose();

        /// <summary>
        /// Disposes the client. Same as Disconnect()
        /// </summary>
        public void Dispose()
        {
            clientStream.Dispose();
            Connected = false;
        }

        /// <summary>
        /// Send a request to the server
        /// </summary>
        /// <param name="data">The data sent to the server</param>
        /// <returns>The response from the server, or null if no response has be sent.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if calling this when the client is not connected.
        /// </exception>
        public Request SendRequest(byte[] data)
        {
            if (Connected)
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
            else
                throw new InvalidOperationException("The client has not been connected yet");
        }

        /// <summary>
        /// Send a request to the server
        /// </summary>
        /// <param name="stringData">The formatted data sent to the server</param>
        /// <returns>The response from the server, or null if no response has be sent.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if calling this when the client is not connected.
        /// </exception>
        public Request SendRequest(string stringData) => SendRequest(Encoding.UTF8.GetBytes(stringData));

        /// <summary>
        /// Starts the client and connects it to the server, or waits until a connection can be done.
        /// </summary>
        public void Start()
        {
            clientStream = new NamedPipeClientStream(".", Key, PipeDirection.InOut);
            clientStream.Connect();
            Connected = true;
        }

        #endregion Public Methods
    }
}