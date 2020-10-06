using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interprocomm
{
    /// <summary>
    /// A request sent by the client.
    /// </summary>
    public class Request
    {
        #region Internal Fields

        internal byte[] data;
        internal byte[] response;

        #endregion Internal Fields

        #region Internal Constructors

        internal Request(byte[] data)
        {
            this.data = data;
            response = null;
        }

        #endregion Internal Constructors

        #region Public Properties

        /// <summary>
        /// The raw content of the response.
        /// </summary>
        public byte[] Data => data;

        /// <summary>
        /// A formatted version of the content. It may not work depending of the raw content, if the
        /// request was not meant to be formatted.
        /// </summary>
        public string StringData => Encoding.UTF8.GetString(data);

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Respond to the request with data.
        /// </summary>
        /// <param name="data">data to respond or null for no response.</param>
        public void Respond(byte[] data) => response = data;

        /// <summary>
        /// Respond to the request with formatted data.
        /// </summary>
        /// <param name="stringData">string to respond.</param>
        public void Respond(string stringData) => response = Encoding.UTF8.GetBytes(stringData);

        #endregion Public Methods
    }
}