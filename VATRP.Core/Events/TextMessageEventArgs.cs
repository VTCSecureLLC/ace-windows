using System;

namespace VATRP.Core.Events
{
    public class TextMessageEventArgs : EventArgs
    {
        public TextMessageEventArgs(string remote)
        {
            this.Remote = remote;
        }

        public string Remote { get; private set; }
    }
}

