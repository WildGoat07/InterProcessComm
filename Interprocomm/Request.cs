using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interprocomm
{
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

        public byte[] Data => data;
        public string StringData => Encoding.UTF8.GetString(data);

        #endregion Public Properties

        #region Public Methods

        public void Respond(byte[] data) => response = data;

        public void Respond(string stringData) => response = Encoding.UTF8.GetBytes(stringData);

        #endregion Public Methods
    }
}