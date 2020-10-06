using System;
using System.Collections.Generic;
using System.Text;

namespace Interprocomm
{
    /// <summary>
    /// A response from the server after sending a request.
    /// </summary>
    public class Response
    {
        #region Internal Fields

        internal byte[] data;

        #endregion Internal Fields

        #region Internal Constructors

        internal Response(byte[] data)
        {
            this.data = data;
        }

        #endregion Internal Constructors

        #region Public Properties

        /// <summary>
        /// The raw content of the response.
        /// </summary>
        public byte[] Data => data;

        /// <summary>
        /// A formatted version of the content. It may not work depending of the raw content, if the
        /// response was not meant to be formatted.
        /// </summary>
        public string StringData => Encoding.UTF8.GetString(data);

        #endregion Public Properties
    }
}