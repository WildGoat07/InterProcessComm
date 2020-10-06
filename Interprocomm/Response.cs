using System;
using System.Collections.Generic;
using System.Text;

namespace Interprocomm
{
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

        public byte[] Data => data;
        public string StringData => Encoding.UTF8.GetString(data);

        #endregion Public Properties
    }
}