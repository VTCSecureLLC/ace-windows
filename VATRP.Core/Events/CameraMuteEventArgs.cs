using System;
using System.Collections.Generic;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class CameraMuteEventArgs : InfoEventBaseArgs
    {
        public bool IsMuted { get; private set; }
        public CameraMuteEventArgs(VATRPCall call, bool muted) :
            base(call)
        {
            IsMuted = muted;
        }
    }
}

