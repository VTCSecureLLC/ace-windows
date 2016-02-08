using System;
using System.Collections.Generic;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class InfoEventBaseArgs : EventArgs
    {
        public VATRPCall ActiveCall { get; private set; }

        protected InfoEventBaseArgs(VATRPCall call)
        {
            ActiveCall = call;
        }
    }
}

