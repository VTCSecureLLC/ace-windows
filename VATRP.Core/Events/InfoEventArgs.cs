using System;
using System.Collections.Generic;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class InfoEventArgs : InfoEventBaseArgs
    {
        private Dictionary<string, string> _infoMap;

        public Dictionary<string, string> InfoMap
        {
            get { return _infoMap ?? (_infoMap = new Dictionary<string, string>()); }

            set { _infoMap = value; }
        }

        public InfoEventArgs(VATRPCall call) :
            base(call)
        {
            
        }
    }
}

