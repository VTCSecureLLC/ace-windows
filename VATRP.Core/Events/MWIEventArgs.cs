using System;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class MWIEventArgs : EventArgs
    {
        public int MwiCount { get; private set; }

        public bool MessageWaiting { get; private set; }
        public MWIEventArgs( int count, bool message_waiting)
        {
            MwiCount = count;
            MessageWaiting = message_waiting;
        }
    }
}

