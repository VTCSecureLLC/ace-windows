using System;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class MWIEventArgs : EventArgs
    {
        public int MwiCount { get; private set; }
        public MWIEventArgs( int count)
        {
            MwiCount = count;
        }
    }
}

