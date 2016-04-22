using System;
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class DeclineMessageArgs : EventArgs
    {
        public string DeclineMessage { get; private set; }

        public string MessageHeader { get; private set; }

        public VATRPContact Sender { get; set; }

        public DeclineMessageArgs(string header, string msg)
        {
            DeclineMessage = msg;
            MessageHeader = header;
        }

    }
}
